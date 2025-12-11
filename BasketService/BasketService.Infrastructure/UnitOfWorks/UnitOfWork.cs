using BasketService.Application.UnitOfWorks;

namespace BasketService.Infrastructure.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    private bool _transactionStarted = false;
    private bool _disposed = false;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Redis'te her işlem anında kaydedildiği için bu metod boş
        // Interface uyumluluğu için korunuyor
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Redis transaction'ları farklı çalışır, burada sadece flag set ediyoruz
        // Gerçek transaction gerekiyorsa Redis transaction API'si kullanılabilir
        _transactionStarted = true;
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Redis'te her işlem anında kaydedildiği için commit işlemi yok
        _transactionStarted = false;
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Redis'te rollback için özel bir mekanizma yok
        // Gerçek transaction gerekiyorsa Redis transaction API'si kullanılabilir
        _transactionStarted = false;
        return Task.CompletedTask;
    }
}

