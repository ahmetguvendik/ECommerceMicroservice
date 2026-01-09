using HealthChecks.UI.Client;
using NLog;
using NLog.Web;
using Shared.Extensions;

// Configure NLog
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Info("Monitoring.Service starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog for Dependency Injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddOpenApi();

// Health Checks
builder.Services.AddHealthChecks();

// Health Checks UI - All configuration in code
var connectionString = "Host=localhost;Port=5432;Database=MonitoringServiceDb;Username=ahmet;Password=Ahmet.123";

builder.Services.AddHealthChecksUI(settings =>
    {
        // Health Check Endpoints
        settings.AddHealthCheckEndpoint("Product Service", "http://localhost:5295/health");
        settings.AddHealthCheckEndpoint("Order Service", "http://localhost:5259/health");
        settings.AddHealthCheckEndpoint("Stock Service", "http://localhost:5142/health");
        settings.AddHealthCheckEndpoint("Basket Service", "http://localhost:5153/health");
        settings.AddHealthCheckEndpoint("Payment Service", "http://localhost:5282/health");
        settings.AddHealthCheckEndpoint("Delivery Service", "http://localhost:5276/health");
        
        // Evaluation time: 10 seconds (her 10 saniyede bir kontrol)
        settings.SetEvaluationTimeInSeconds(5);
        
        // Minimum seconds between failure notifications: 60 seconds
        settings.SetMinimumSecondsBetweenFailureNotifications(60);
    })
    .AddPostgreSqlStorage(connectionString);

    var app = builder.Build();

    // Add Correlation ID middleware (should be early in the pipeline)
    app.UseCorrelationId();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Health Checks UI
app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-ui-api";
});

    logger.Info("Monitoring.Service started successfully");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Monitoring.Service stopped because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}