using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Contexts;

namespace OrderService.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T>  where T : BaseEntity{
    private readonly OrderServiceDbContext  _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(OrderServiceDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    
    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return null;
        }
        return await GetByIdAsync(guidId, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
        if (entity != null && entity.IsDeleted)
        {
            return null;
        }
        return entity;
    }

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedTime = DateTime.UtcNow;
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.ModifiedTime = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.ModifiedTime = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public async Task<T?> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null || !entity.IsDeleted)
        {
            return null;
        }
        
        entity.IsDeleted = false;
        entity.ModifiedTime = DateTime.UtcNow;
        _dbSet.Update(entity);
        return entity;
    }
    
    public async Task<List<T>> GetDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.IgnoreQueryFilters().Where(x => x.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);
    }
    

    public IQueryable<T> GetQueryable()
    {
        return _dbSet;
    }
}
