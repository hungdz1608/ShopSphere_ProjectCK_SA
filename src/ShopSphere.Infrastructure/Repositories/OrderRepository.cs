using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.db;

namespace ShopSphere.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ShopSphereDbContext _db;

    public OrderRepository(ShopSphereDbContext db)
    {
        _db = db;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
