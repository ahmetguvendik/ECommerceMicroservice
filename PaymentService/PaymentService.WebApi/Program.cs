using PaymentService.Application;
using PaymentService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

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

app.UseSwagger();
app.UseSwaggerUI();

// Health Check Endpoint
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();