using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application;

public static class ServiceRegistration
{
    public static void AddApplicationService(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssemblies(typeof(ServiceRegistration).Assembly);
            cfg.NotificationPublisherType = typeof(MediatR.NotificationPublishers.ForeachAwaitPublisher);
        });
        service.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);
        service.AddTransient(typeof(IPipelineBehavior<,>), typeof(Validations.Behaviors.ValidationBehavior<,>));
    }
}
