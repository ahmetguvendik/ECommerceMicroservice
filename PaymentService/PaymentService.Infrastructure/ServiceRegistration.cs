using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Consumers;
using PaymentService.Infrastructure.Contexts;
using PaymentService.Infrastructure.Services;
using Shared;

namespace PaymentService.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<PaymentServiceDbContext>(opt => opt.UseNpgsql(connectionString));

        services.AddScoped<IPaymentEventService, PaymentEventService>();

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<PaymentStartedEventConsumer>();

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(configuration.GetConnectionString("RabbitMq"));

                bus.ReceiveEndpoint(RabbitMqSettings.Payment_StartedEvenetQueue, e =>
                {
                    e.ConfigureConsumer<PaymentStartedEventConsumer>(context);
                });
            });
        });
    }
}

