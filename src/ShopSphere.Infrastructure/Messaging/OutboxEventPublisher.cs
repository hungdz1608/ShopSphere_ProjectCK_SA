using System.Text.Json;
using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;

namespace ShopSphere.Infrastructure.Messaging;

public class OutboxEventPublisher : IEventPublisher
{
    private readonly IOutboxRepository _outboxRepository;

    public OutboxEventPublisher(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task PublishAsync(string eventName, object payload, string? messageId = null)
    {
        var id = Guid.TryParse(messageId, out var parsed) ? parsed : Guid.NewGuid();

        var message = new OutboxMessage
        {
            Id = id,
            EventName = eventName,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAt = DateTime.UtcNow,
            Status = "Pending",
            RetryCount = 0
        };

        await _outboxRepository.AddAsync(message);
    }
}
