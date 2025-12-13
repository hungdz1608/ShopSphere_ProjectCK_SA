namespace ShopSphere.Domain.Entities;

public enum OrderStatus
{
    PendingPayment,
    PaymentCompleted,
    Cancelled,
    Failed
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BuyerEmail { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
