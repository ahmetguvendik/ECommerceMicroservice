using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Repositories;
using ProductService.Application.UnitOfWorks;
using ProductService.Infrastructure.Contexts;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.UnitOfWorks;

namespace ProductService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddPersistenceService(this IServiceCollection collection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        collection.AddDbContext<ProductServiceDbContext>(opt =>
            opt.UseNpgsql(connectionString));
        
        //Repositories
        collection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        collection.AddTransient<IUnitOfWork, UnitOfWork>();
        
        //Mass Trabsit (Rabbitmq)
        collection.AddMassTransit(c => c.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(configuration.GetConnectionString("RabbitMq"));
        }));
    }
}