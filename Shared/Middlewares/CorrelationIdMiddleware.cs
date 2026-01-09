using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;

namespace Shared.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add to HttpContext.Items for access throughout the request
        context.Items["CorrelationId"] = correlationId;
        
        // Add to NLog MappedDiagnosticsLogicalContext (MDLC) for automatic inclusion in logs
        MappedDiagnosticsLogicalContext.Set("CorrelationId", correlationId);
        
        // Add to response headers so clients can track their requests
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // Removed the log message - CorrelationId is already in MDLC for all logs
        // _logger.LogInformation("Request started with CorrelationId: {CorrelationId}", correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            // Clean up MDLC after request
            MappedDiagnosticsLogicalContext.Remove("CorrelationId");
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get from request header
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Create new one if not found
        return Guid.NewGuid().ToString();
    }
}
