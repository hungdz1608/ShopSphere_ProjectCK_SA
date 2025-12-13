using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Errors;

namespace ShopSphere.Application.Services;

public class CheckoutService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEventPublisher _publisher;

    public CheckoutService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IEventPublisher publisher)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _publisher = publisher;
    }

    public async Task<Order> StartCheckoutAsync(string buyerEmail, decimal total, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(buyerEmail))
            throw new BadRequestException("Buyer email is required.");
        if (total <= 0)
            throw new BadRequestException("Total amount must be > 0.");

        var order = new Order
        {
            BuyerEmail = buyerEmail.Trim(),
            TotalAmount = total,
            Status = OrderStatus.PendingPayment
        };

        order = await _orderRepository.AddAsync(order, cancellationToken);

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = total,
            Status = PaymentStatus.Pending
        };
        await _paymentRepository.AddAsync(payment, cancellationToken);

        await _publisher.PublishAsync("order.created", new
        {
            order.Id,
            order.BuyerEmail,
            order.TotalAmount,
            order.Status,
            paymentId = payment.Id
        }, order.Id.ToString());

        return order;
    }

    public async Task<Order> MarkPaymentCompletedAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new NotFoundException("Order not found");

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null) throw new NotFoundException("Payment not found");

        payment.Status = PaymentStatus.Completed;
        payment.UpdatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.PaymentCompleted;
        order.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _publisher.PublishAsync("payment.completed", new
        {
            order.Id,
            payment.Id,
            payment.Amount,
            order.Status
        }, payment.Id.ToString());

        return order;
    }

    public async Task<Order> FailPaymentAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new NotFoundException("Order not found");

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null) throw new NotFoundException("Payment not found");

        payment.Status = PaymentStatus.Failed;
        payment.UpdatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.Failed;
        order.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _publisher.PublishAsync("payment.failed", new
        {
            order.Id,
            payment.Id,
            payment.Amount,
            reason
        }, payment.Id.ToString());

        return order;
    }

    public async Task<Order> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new NotFoundException("Order not found");
        return order;
    }
}
