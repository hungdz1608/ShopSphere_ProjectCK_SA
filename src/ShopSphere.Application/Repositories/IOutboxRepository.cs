using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Repositories;

public interface IOutboxRepository
{
    Task<OutboxMessage> AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetPendingAsync(int take, CancellationToken cancellationToken);
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken);
}
