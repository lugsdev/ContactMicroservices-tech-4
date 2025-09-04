using System.Text;
using System.Text.Json;
using Common.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Common.Messaging
{
	public class RabbitMQPublisher : IMessagePublisher, IAsyncDisposable
	{
		private readonly ILogger<RabbitMQPublisher> _logger;
		private readonly IConnection _connection;
		private readonly IChannel _channel;
		private readonly string _exchangeName;
		private bool _disposed;

		public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
		{
			_logger = logger;
			_exchangeName = configuration.GetValue<string>("RabbitMQ:ExchangeName") ?? "contact_events";

			try
			{
				var factory = new ConnectionFactory()
				{
					//DispatchConsumersAsync = true,
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
				).GetAwaiter().GetResult();

				_logger.LogInformation("RabbitMQ Publisher conectado. Exchange: {ExchangeName}", _exchangeName);
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

		public async Task PublishAsync<T>(T message, string routingKey = "") where T : ContactEvent
		{
			try
			{
				if (_disposed || _connection is null || !_connection.IsOpen || _channel is null || !_channel.IsOpen)
				{
					_logger.LogWarning("RabbitMQ não está conectado. Evento não será publicado.");
					return;
				}

				if (string.IsNullOrEmpty(routingKey))
					routingKey = $"contact.{message.EventType.ToLower()}";

				var messageBody = JsonSerializer.Serialize(message, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
				var body = Encoding.UTF8.GetBytes(messageBody);
				ReadOnlyMemory<byte> memoryBody = body;

				await _channel.BasicPublishAsync(
					exchange: _exchangeName,
					routingKey: routingKey,
					body: body
				);

				_logger.LogInformation("Evento publicado: {EventType} para contato {ContactId} com routing key {RoutingKey}. Payload: {Payload}",
					message.EventType, message.ContactId, routingKey, messageBody);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao publicar evento {EventType} para contato {ContactId}",
					message.EventType, message.ContactId);
				throw;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_disposed) return;

			try
			{
				if (_channel is not null)
				{
					await _channel.CloseAsync();
					await _channel.DisposeAsync();
				}

				if (_connection is not null)
				{
					await _connection.CloseAsync();
					await _connection.DisposeAsync();
				}

				_logger.LogInformation("RabbitMQ Publisher desconectado.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao desconectar RabbitMQ Publisher.");
			}

			_disposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
