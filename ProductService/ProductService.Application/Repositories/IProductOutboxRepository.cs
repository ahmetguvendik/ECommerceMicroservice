using ProductService.Domain.Entities;

namespace ProductService.Application.Repositories;

public interface IProductOutboxRepository
{
    Task CreateAsync(ProductOutbox outbox, CancellationToken cancellationToken = default);
}

