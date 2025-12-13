namespace ShopSphere.Domain.Entities;

public class ProcessedMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MessageId { get; set; } = default!;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
