namespace ShopSphere.Application.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(string eventName, object payload, string? messageId = null);
}
