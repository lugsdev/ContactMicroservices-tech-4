using Common.Events;

namespace Common.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string routingKey = "") where T : ContactEvent;
        //void Publish<T>(T message, string routingKey = "") where T : ContactEvent;
    }
}

