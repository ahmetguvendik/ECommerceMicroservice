using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BasketService.Application.Repositories;
using BasketService.Application.UnitOfWorks;
using BasketService.Domain.Entities;
using BasketService.Infrastructure.Contexts;
using BasketService.Infrastructure.Repositories;
using BasketService.Infrastructure.UnitOfWorks;

namespace BasketService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddPersistenceService(this IServiceCollection collection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        collection.AddDbContext<BasketServiceDbContext>(opt =>
            opt.UseNpgsql(connectionString));
        
        collection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        collection.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}

