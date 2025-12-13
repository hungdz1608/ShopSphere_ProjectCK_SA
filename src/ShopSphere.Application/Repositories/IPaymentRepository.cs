using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Repositories;

public interface IPaymentRepository
{
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken);
}
