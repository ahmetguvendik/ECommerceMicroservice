using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Repositories;
using OrderService.Application.UnitOfWorks;
using OrderService.Infrastructure.Contexts;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.UnitOfWorks;

namespace OrderService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddPersistenceService(this IServiceCollection collection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        collection.AddDbContext<OrderServiceDbContext>(opt =>
            opt.UseNpgsql(connectionString));
        
        //Repositories
        collection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));        
        collection.AddScoped<IUnitOfWork, UnitOfWork>();
        
        //Services
        
        //Mass Transit (Rabbitmq)
        collection.AddMassTransit(c => c.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(configuration.GetConnectionString("RabbitMq"));
        }));
    }
}