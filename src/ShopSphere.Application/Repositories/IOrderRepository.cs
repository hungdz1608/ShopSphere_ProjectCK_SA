using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Repositories;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Order order, CancellationToken cancellationToken);
}
