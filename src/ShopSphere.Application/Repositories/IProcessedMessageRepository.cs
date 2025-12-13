using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Repositories;

public interface IProcessedMessageRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken);
    Task AddAsync(ProcessedMessage message, CancellationToken cancellationToken);
}
