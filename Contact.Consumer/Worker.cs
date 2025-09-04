using Common.Events;

namespace Contact.Consumer
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;

		public Worker(ILogger<Worker> logger)
		{
			_logger = logger;
		}

		public interface IMessageHandler<T> where T : ContactEvent
		{
			Task HandleAsync(T message, string routingKey);
		}
		public interface IMessageConsumer : IAsyncDisposable
		{
			Task StartConsumingAsync<T>(IMessageHandler<T> handler, string queueName = "", string routingKey = "*") where T : ContactEvent;
			Task StopConsumingAsync();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				}
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}
