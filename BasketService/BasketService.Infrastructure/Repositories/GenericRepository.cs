using System.Linq.Expressions;
using BasketService.Application.Repositories;
using BasketService.Application.Services;
using BasketService.Domain.Entities;
using BasketService.Infrastructure.Services;

namespace BasketService.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly IRedisService _redisService;
    private readonly string _keyPrefix;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromHours(1); // Varsayılan 1 saat

    public GenericRepository(IRedisService redisService)
    {
        _redisService = redisService;
        // Entity tipine göre key prefix belirle
        _keyPrefix = typeof(T).Name.ToLower() + ":";
    }

    private string GetKey(Guid id) => $"{_keyPrefix}{id}";
    private string GetKey(string id) => $"{_keyPrefix}{id}";

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Redis'te tüm key'leri bulmak için pattern kullan
        // Not: Bu production'da performans sorunu yaratabilir, gerekirse index kullanılmalı
        if (_redisService is RedisService redisService)
        {
            var database = redisService.GetDatabase();
            var endpoints = database.Multiplexer.GetEndPoints();
            if (endpoints.Length == 0)
                return new List<T>();
                
            var server = database.Multiplexer.GetServer(endpoints.First());
            var keys = server.Keys(pattern: $"{_keyPrefix}*");
            
            var results = new List<T>();
            foreach (var key in keys)
            {
                var entity = await _redisService.GetAsync<T>(key.ToString());
                if (entity != null && !entity.IsDeleted)
                {
                    results.Add(entity);
                }
            }
            
            return results;
        }
        
        throw new InvalidOperationException("RedisService implementation not found");
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile()).ToList();
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
        var key = GetKey(id);
        var entity = await _redisService.GetAsync<T>(key);
        
        if (entity != null && entity.IsDeleted)
        {
            return null;
        }
        
        return entity;
    }

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedTime = DateTime.UtcNow;
        var key = GetKey(entity.Id);
        await _redisService.SetAsync(key, entity, _defaultExpiry);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.ModifiedTime = DateTime.UtcNow;
        var key = GetKey(entity.Id);
        return _redisService.SetAsync(key, entity, _defaultExpiry);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.ModifiedTime = DateTime.UtcNow;
        var key = GetKey(entity.Id);
        return _redisService.SetAsync(key, entity, _defaultExpiry);
    }

    public IQueryable<T> GetQueryable()
    {
        // Redis için IQueryable desteği yok, bu yüzden GetAllAsync kullanılmalı
        throw new NotSupportedException("IQueryable is not supported with Redis. Use GetAllAsync instead.");
    }
}
