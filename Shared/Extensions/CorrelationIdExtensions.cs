using Microsoft.AspNetCore.Http;
using NLog;

namespace Shared.Extensions;

public static class CorrelationIdExtensions
{
    private const string CorrelationIdKey = "CorrelationId";
    
    /// <summary>
    /// Gets the current CorrelationId from HttpContext
    /// </summary>
    public static string? GetCorrelationId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CorrelationIdKey, out var correlationId))
        {
            return correlationId?.ToString();
        }
        return null;
    }
    
    /// <summary>
    /// Gets the current CorrelationId from HttpContext or creates a new one
    /// </summary>
    public static string GetOrCreateCorrelationId(this HttpContext httpContext)
    {
        var correlationId = httpContext.GetCorrelationId();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            httpContext.Items[CorrelationIdKey] = correlationId;
        }
        return correlationId;
    }
    
    /// <summary>
    /// Sets CorrelationId to NLog MDLC (MappedDiagnosticsLogicalContext)
    /// Useful for background jobs and consumers
    /// </summary>
    public static void SetCorrelationIdToNLog(string correlationId)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            MappedDiagnosticsLogicalContext.Set(CorrelationIdKey, correlationId);
        }
    }
    
    /// <summary>
    /// Clears CorrelationId from NLog MDLC
    /// </summary>
    public static void ClearCorrelationIdFromNLog()
    {
        MappedDiagnosticsLogicalContext.Remove(CorrelationIdKey);
    }
}
