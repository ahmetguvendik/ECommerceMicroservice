using Microsoft.EntityFrameworkCore;
using StockService.Application.Repositories;
using StockService.Domain.Entities;
using StockService.Infrastructure.Contexts;

namespace StockService.Infrastructure.Repositories;

public class StockInboxRepository : IStockInboxRepository
{
    private readonly StockServiceDbContext _context;

    public StockInboxRepository(StockServiceDbContext context)
    {
        _context = context;
    }

    public async Task<StockInbox?> GetByIdAsync(Guid idempotentToken, CancellationToken cancellationToken = default)
    {
        return await _context.StockInboxes.FirstOrDefaultAsync(x => x.IdempotentToken == idempotentToken, cancellationToken);
    }

    public async Task CreateAsync(StockInbox inbox, CancellationToken cancellationToken = default)
    {
        await _context.StockInboxes.AddAsync(inbox, cancellationToken);
    }

    public Task UpdateAsync(StockInbox inbox, CancellationToken cancellationToken = default)
    {
        _context.StockInboxes.Update(inbox);
        return Task.CompletedTask;
    }
}

