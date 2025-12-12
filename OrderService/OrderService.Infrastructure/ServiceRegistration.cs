using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Repositories;
using OrderService.Application.Services;
using OrderService.Application.UnitOfWorks;
using OrderService.Infrastructure.Contexts;
using OrderService.Infrastructure.Consumers;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Services;
using OrderService.Infrastructure.UnitOfWorks;
using Shared;

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
        collection.AddScoped<IOrderEventService, OrderEventService>();
        
        //Mass Transit (Rabbitmq)
        collection.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<OrderStartedEventConsumer>();

            cfg.UsingRabbitMq((context, hostConfig) =>
            {
                hostConfig.Host(configuration.GetConnectionString("RabbitMq"));
                hostConfig.ReceiveEndpoint(RabbitMqSettings.Stock_OrderStartedEventQueue, endpoint =>
                {
                    endpoint.ConfigureConsumer<OrderStartedEventConsumer>(context);
                });
            });
        });
    }
}