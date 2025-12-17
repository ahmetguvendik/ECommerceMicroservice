using DeliveryService.Application.Services;
using DeliveryService.Infrastructure.Consumers;
using DeliveryService.Infrastructure.Contexts;
using DeliveryService.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

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

                bus.ReceiveEndpoint(RabbitMqSettings.Delivery_StartedEventQueue, e =>
                {
                    e.ConfigureConsumer<DeliveryStartedEventConsumer>(context);
                });
            });
        });
    }
}

