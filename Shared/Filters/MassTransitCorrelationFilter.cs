using MassTransit;
using MassTransit.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using Shared.Extensions;

namespace Shared.Filters;

/// <summary>
/// MassTransit consume filter to automatically set CorrelationId for all consumers
/// </summary>
public class MassTransitCorrelationFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<MassTransitCorrelationFilter<T>> _logger;

    public MassTransitCorrelationFilter(ILogger<MassTransitCorrelationFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // Get CorrelationId from MassTransit context
        var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();
        
        // Set to NLog MDLC
        CorrelationIdExtensions.SetCorrelationIdToNLog(correlationId);
        
        // Log message received for tracking event flow across services
        _logger.LogInformation(
            "🔔 Event Received: {MessageType} | CorrelationId: {CorrelationId}",
            typeof(T).Name,
            correlationId);

        try
        {
            await next.Send(context);
            
            // Log successful processing
            _logger.LogInformation(
                "✅ Event Processed: {MessageType} | CorrelationId: {CorrelationId}",
                typeof(T).Name,
                correlationId);
        }
        catch (Exception ex)
        {
            // Log failed processing
            _logger.LogError(ex,
                "❌ Event Failed: {MessageType} | CorrelationId: {CorrelationId}",
                typeof(T).Name,
                correlationId);
            throw;
        }
        finally
        {
            // Clean up after processing
            CorrelationIdExtensions.ClearCorrelationIdFromNLog();
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlationIdFilter");
    }
}

/// <summary>
/// Configure specification for the correlation filter
/// </summary>
public class MassTransitCorrelationFilterSpecification<T> : IPipeSpecification<ConsumeContext<T>> where T : class
{
    private readonly ILogger<MassTransitCorrelationFilter<T>> _logger;

    public MassTransitCorrelationFilterSpecification(ILogger<MassTransitCorrelationFilter<T>> logger)
    {
        _logger = logger;
    }

    public void Apply(IPipeBuilder<ConsumeContext<T>> builder)
    {
        builder.AddFilter(new MassTransitCorrelationFilter<T>(_logger));
    }

    public IEnumerable<ValidationResult> Validate()
    {
        return Enumerable.Empty<ValidationResult>();
    }
}
