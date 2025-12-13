using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.db;

namespace ShopSphere.Infrastructure.Repositories;

public class ProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly ShopSphereDbContext _db;

    public ProcessedMessageRepository(ShopSphereDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken)
    {
        return _db.ProcessedMessages.AnyAsync(m => m.MessageId == messageId, cancellationToken);
    }

    public async Task AddAsync(ProcessedMessage message, CancellationToken cancellationToken)
    {
        _db.ProcessedMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
