using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.Application;

public static class ServiceRegistration
{
    public static void AddApplicationService(this IServiceCollection services, IConfiguration configuration)
    {
        // Placeholder for MediatR/FluentValidation if needed later
    }
}

