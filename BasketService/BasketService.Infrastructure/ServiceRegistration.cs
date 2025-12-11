using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using BasketService.Application.Repositories;
using BasketService.Application.Services;
using BasketService.Application.UnitOfWorks;
using BasketService.Infrastructure.Repositories;
using BasketService.Infrastructure.Services;
using BasketService.Infrastructure.UnitOfWorks;

namespace BasketService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddPersistenceService(this IServiceCollection collection, IConfiguration configuration)
    {
        // Redis bağlantısı
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        collection.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        
        // Redis Service
        collection.AddScoped<IRedisService, RedisService>();
        
        // Repository'ler
        collection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        // UnitOfWork (Redis için basitleştirilmiş)
        collection.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // HTTP Client Services (External Services)
        collection.AddHttpClient<IProductService, Infrastructure.Services.ProductService>();
        collection.AddHttpClient<IStockService, Infrastructure.Services.StockService>();
    }
}

