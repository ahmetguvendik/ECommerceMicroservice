using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Contexts;

namespace ProductService.Infrastructure.Repositories;

public class ProductOutboxRepository : IProductOutboxRepository
{
    private readonly ProductServiceDbContext _context;

    public ProductOutboxRepository(ProductServiceDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(ProductOutbox outbox, CancellationToken cancellationToken = default)
    {
        await _context.ProductOutboxes.AddAsync(outbox, cancellationToken);
    }
}

