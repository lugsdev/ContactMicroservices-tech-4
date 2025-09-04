using System.Text;
using System.Text.Json;
using Common.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Common.Messaging
{
	public interface IMessageHandler<T> where T : ContactEvent
	{
		Task HandleAsync(T message, string routingKey);
	}
	public interface IMessageConsumer : IAsyncDisposable
	{
		Task StartConsumingAsync<T>(IMessageHandler<T> handler, string queueName = "", string routingKey = "*") where T : ContactEvent;
		Task StopConsumingAsync();
	}

	public class RabbitMQConsumer: IMessageConsumer
	{
		private readonly ILogger<RabbitMQConsumer> _logger;
		private readonly IConnection _connection;
		private readonly IChannel _channel;
		private readonly string _exchangeName;
		private readonly List<string> _consumerTags;
		private bool _disposed;
		private bool _isConsuming;

		public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger)
		{
			_logger = logger;
			_exchangeName = configuration.GetValue<string>("RabbitMQ:ExchangeName") ?? "contact_events";
			_consumerTags = new List<string>();

			try
			{
				var factory = new ConnectionFactory()
				{
					HostName = configuration.GetValue<string>("RabbitMQ:HostName") ?? "localhost",
					Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
					UserName = configuration.GetValue<string>("RabbitMQ:UserName") ?? "guest",
					Password = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest",
					VirtualHost = configuration.GetValue<string>("RabbitMQ:VirtualHost") ?? "/"
				};

				_connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
				_channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

				_channel.ExchangeDeclareAsync(
					exchange: _exchangeName,
					type: ExchangeType.Fanout,
					durable: true
				);

				_logger.LogInformation("RabbitMQ Consumer conectado. Exchange: {ExchangeName}", _exchangeName);
			}
			catch (BrokerUnreachableException ex)
			{
				_logger.LogError(ex, "RabbitMQ não encontrado. Verifique se o servidor está em execução.");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao conectar com RabbitMQ.");
				throw;
			}
		}

		public async Task StartConsumingAsync<T>(IMessageHandler<T> handler, string queueName = "", string routingKey = "*") where T : ContactEvent
		{
			if (_disposed || _connection is null || !_connection.IsOpen || _channel is null || !_channel.IsOpen)
			{
				_logger.LogWarning("RabbitMQ não está conectado. Consumer não pode ser iniciado.");
				return;
			}

			if (string.IsNullOrEmpty(queueName))
				queueName = $"queue_{typeof(T).Name}";

			_channel.QueueDeclareAsync(
				queue: queueName,
				durable: true,
				exclusive: false,
				autoDelete: false
			);

			_channel.QueueBindAsync(
				queue: queueName,
				exchange: _exchangeName,
				routingKey: routingKey
			);

			var consumer = new AsyncEventingBasicConsumer(_channel);
			consumer.ReceivedAsync += async (model, ea) =>
			{
				try
				{
					var messageJson = Encoding.UTF8.GetString(ea.Body.ToArray());
					var message = JsonSerializer.Deserialize<T>(messageJson, new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					});

					if (message != null)
					{
						await handler.HandleAsync(message, ea.RoutingKey);
						await _channel.BasicAckAsync(ea.DeliveryTag, false);

						_logger.LogInformation("Evento processado: {EventType} para contato {ContactId} com routing key {RoutingKey}",
							message.EventType, message.ContactId, ea.RoutingKey);
					}
					else
					{
						_logger.LogWarning("Não foi possível deserializar a mensagem: {Message}", messageJson);
						await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Erro ao processar mensagem com routing key {RoutingKey}", ea.RoutingKey);
					await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
				}
			};

			var consumerTag = await _channel.BasicConsumeAsync(
				queue: queueName,
				autoAck: false,
				consumer: consumer
			);

			_consumerTags.Add(consumerTag);
			_isConsuming = true;

			_logger.LogInformation("Consumer iniciado para fila {QueueName} com routing key {RoutingKey}. Consumer tag: {ConsumerTag}",
				queueName, routingKey, consumerTag);
		}

		public async Task StopConsumingAsync()
		{
			if (!_isConsuming || _channel is null || !_channel.IsOpen) return;

			foreach (var consumerTag in _consumerTags)
			{
				await _channel.BasicCancelAsync(consumerTag);
				_logger.LogInformation("Consumer cancelado: {ConsumerTag}", consumerTag);
			}

			_consumerTags.Clear();
			_isConsuming = false;
			_logger.LogInformation("Todos os consumers foram parados.");
		}

		public async ValueTask DisposeAsync()
		{
			if (_disposed) return;

			try
			{
				await StopConsumingAsync();

				_channel?.Dispose();
				_connection?.Dispose();

				_logger.LogInformation("RabbitMQ Consumer desconectado.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao desconectar RabbitMQ Consumer.");
			}

			_disposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
