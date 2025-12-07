using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.Application;


public static class ServiceRegistration
{
    public static void AddApplicationService(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssemblies(typeof(ServiceRegistration).Assembly);
            // Disable MediatR's built-in logging
            cfg.NotificationPublisherType = typeof(MediatR.NotificationPublishers.ForeachAwaitPublisher);
        });
        // Register FluentValidation validators and MediatR validation pipeline
        service.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);
        service.AddTransient(typeof(IPipelineBehavior<,>), typeof(Validations.Behaviors.ValidationBehavior<,>));
    }
    
}