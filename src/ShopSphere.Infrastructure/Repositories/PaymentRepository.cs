using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.db;

namespace ShopSphere.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ShopSphereDbContext _db;

    public PaymentRepository(ShopSphereDbContext db)
    {
        _db = db;
    }

    public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
