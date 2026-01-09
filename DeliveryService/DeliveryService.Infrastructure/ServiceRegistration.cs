using DeliveryService.Application.Services;
using DeliveryService.Infrastructure.Consumers;
using DeliveryService.Infrastructure.Contexts;
using DeliveryService.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Filters;

namespace DeliveryService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<DeliveryServiceDbContext>(opt => opt.UseNpgsql(connectionString));

        services.AddScoped<IDeliveryEventService, DeliveryEventService>();

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<DeliveryStartedEventConsumer>();

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(configuration.GetConnectionString("RabbitMq"));
                
                // Global Correlation ID filter for all consumers
                bus.UseConsumeFilter(typeof(MassTransitCorrelationFilter<>), context);

                bus.ReceiveEndpoint(RabbitMqSettings.Delivery_StartedEventQueue, e =>
                {
                    e.ConfigureConsumer<DeliveryStartedEventConsumer>(context);
                });
            });
        });
    }
}

