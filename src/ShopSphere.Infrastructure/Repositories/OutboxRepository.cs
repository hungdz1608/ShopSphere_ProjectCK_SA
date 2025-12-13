using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.db;

namespace ShopSphere.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly ShopSphereDbContext _db;

    public OutboxRepository(ShopSphereDbContext db) => _db = db;

    public async Task<OutboxMessage> AddAsync(OutboxMessage message)
    {
        _db.OutboxMessages.Add(message);
        await _db.SaveChangesAsync();
        return message;
    }

    public Task<List<OutboxMessage>> GetPendingAsync(int take, CancellationToken cancellationToken)
    {
        return _db.OutboxMessages
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        _db.OutboxMessages.Update(message);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
