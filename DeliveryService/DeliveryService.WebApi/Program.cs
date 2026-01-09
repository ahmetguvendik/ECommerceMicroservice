using DeliveryService.Application;
using DeliveryService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using NLog;
using NLog.Web;
using Shared.Extensions;

// Configure NLog
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Info("DeliveryService starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog for Dependency Injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks Configuration
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var rabbitMqConnection = builder.Configuration.GetConnectionString("RabbitMq");

if (string.IsNullOrWhiteSpace(defaultConnection))
    throw new InvalidOperationException("DefaultConnection connection string is not configured.");

if (string.IsNullOrWhiteSpace(rabbitMqConnection))
    throw new InvalidOperationException("RabbitMq connection string is not configured.");

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: defaultConnection,
        name: "PostgreSQL",
        tags: new[] { "db", "postgresql", "ready" },
        timeout: TimeSpan.FromSeconds(5))
    .AddCheck("RabbitMQ", () =>
    {
        try
        {
            var uri = new Uri(rabbitMqConnection);
            if (uri.Scheme.StartsWith("amqp"))
            {
                return HealthCheckResult.Healthy("RabbitMQ connection string is valid");
            }
            return HealthCheckResult.Unhealthy("Invalid RabbitMQ connection string");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection error", ex);
        }
    }, tags: new[] { "messaging", "rabbitmq", "ready" });

    builder.Services.AddApplicationService(builder.Configuration);
    builder.Services.AddInfrastructureService(builder.Configuration);

    var app = builder.Build();

    // Add Correlation ID middleware (should be early in the pipeline)
    app.UseCorrelationId();

// Health Check Endpoint - Must be before HTTPS redirect
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.UseSwagger();
app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.MapControllers();

    logger.Info("DeliveryService started successfully");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "DeliveryService stopped because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}

