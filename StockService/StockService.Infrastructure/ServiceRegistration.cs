using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockService.Application.Repositories;
using StockService.Application.Services;
using StockService.Application.UnitOfWorks;
using StockService.Infrastructure.Consumers;
using StockService.Infrastructure.Contexts;
using StockService.Infrastructure.Repositories;
using StockService.Infrastructure.Services;
using StockService.Infrastructure.UnitOfWorks;
using Shared;

namespace StockService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddPersistenceService(this IServiceCollection collection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        collection.AddDbContext<StockServiceDbContext>(opt =>
            opt.UseNpgsql(connectionString));
        
        //Repositories
        collection.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        collection.AddScoped<IStockInboxRepository, StockInboxRepository>();
        collection.AddScoped<IUnitOfWork, UnitOfWork>();
        
        //Application Services
        collection.AddScoped<IProductEventService, ProductEventService>();
        collection.AddScoped<IOrderEventService, OrderEventService>();
        collection.AddScoped<IStockEventService, StockEventService>();

        
        //Mass Transit (Rabbitmq)
        collection.AddMassTransit(c =>
        {
            // Consumer'ları register et
            c.AddConsumer<ProductCreatedEventConsumer>();
            c.AddConsumer<ProductUpdatedEventConsumer>();
            c.AddConsumer<ProductDeletedEventConsumer>();
            c.AddConsumer<OrderCreatedEventConsumer>();
            c.AddConsumer<StockRollbackMessageConsumer>();
            
            c.UsingRabbitMq((context, cfg) =>
            {
                //Host eklenir
                cfg.Host(configuration.GetConnectionString("RabbitMq"));
                
                // Consumer endpoint'lerini yapılandır
                cfg.ReceiveEndpoint(RabbitMqSettings.Stock_ProductCreatedEventQueue, e =>
                {
                    e.ConfigureConsumer<ProductCreatedEventConsumer>(context);
                });
                
                cfg.ReceiveEndpoint(RabbitMqSettings.Stock_ProductUpdatedEventQueue, e =>
                {
                    e.ConfigureConsumer<ProductUpdatedEventConsumer>(context);
                });
                
                cfg.ReceiveEndpoint(RabbitMqSettings.Stock_ProductDeletedEventQueue, e =>
                {
                    e.ConfigureConsumer<ProductDeletedEventConsumer>(context);
                });

                cfg.ReceiveEndpoint(RabbitMqSettings.Stock_OrderCreatedEventQueue, e =>
                {
                    e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
                });

                cfg.ReceiveEndpoint(RabbitMqSettings.Stock_RollbackMessageEventQueue, e =>
                {
                    e.ConfigureConsumer<StockRollbackMessageConsumer>(context);
                });
            });
        }); 
    }
}
