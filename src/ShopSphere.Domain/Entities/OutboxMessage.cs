namespace ShopSphere.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventName { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Error { get; set; }
}
