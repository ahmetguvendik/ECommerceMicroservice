using StockService.Domain.Entities;

namespace StockService.Application.Repositories;

public interface IStockInboxRepository
{
    Task<StockInbox?> GetByIdAsync(Guid idempotentToken, CancellationToken cancellationToken = default);
    Task CreateAsync(StockInbox inbox, CancellationToken cancellationToken = default);
    Task UpdateAsync(StockInbox inbox, CancellationToken cancellationToken = default);
}

