using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Application.UnitOfWorks;
using OrderService.Infrastructure.Contexts;

namespace OrderService.Infrastructure.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderServiceDbContext _dbContext;
    private IDbContextTransaction? _dbContextTransaction;

    public UnitOfWork(OrderServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void Dispose()
    {
        _dbContextTransaction?.Dispose();
        _dbContext.Dispose();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken); 
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContextTransaction != null)
        {
            return;
        }
        _dbContextTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContextTransaction == null)
        {
            return;
        }
        await _dbContextTransaction.CommitAsync(cancellationToken);
        await _dbContextTransaction.DisposeAsync();
        _dbContextTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContextTransaction == null)
        {
            return;
        }
        await _dbContextTransaction.RollbackAsync(cancellationToken);
        await _dbContextTransaction.DisposeAsync();
        _dbContextTransaction = null;
    }
}
