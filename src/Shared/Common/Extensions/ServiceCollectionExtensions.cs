using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Common.Messaging;

namespace Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
            return services;
        }
    }
}

