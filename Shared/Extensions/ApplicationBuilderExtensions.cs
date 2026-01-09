using Microsoft.AspNetCore.Builder;
using Shared.Middlewares;

namespace Shared.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds CorrelationId middleware to the application pipeline
    /// Should be added early in the pipeline to ensure all requests have a CorrelationId
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
