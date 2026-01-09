using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContexts;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;
using NLog;
using NLog.Extensions.Logging;
using Shared.Extensions;

// Configure NLog
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Info("SagaStateMachine.Service starting up...");

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Configure NLog for Dependency Injection
    builder.Logging.ClearProviders();
    builder.Logging.AddNLog();

var rabbitMqHost = builder.Configuration.GetConnectionString("RabbitMq") ?? builder.Configuration["RabbitMQ"];
if (string.IsNullOrWhiteSpace(rabbitMqHost))
{
    throw new InvalidOperationException("RabbitMQ host configuration is missing.");
}

builder.Services.AddMassTransit(config =>
{
    config.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
    {
        opt.ConcurrencyMode = ConcurrencyMode.Optimistic;
        opt.AddDbContext<DbContext, OrderStateDbContext>((provider, options) =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
    });
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitMqHost);
        cfg.ConfigureEndpoints(ctx);
    });
});

    var host = builder.Build();
    
    logger.Info("SagaStateMachine.Service started successfully");
    host.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "SagaStateMachine.Service stopped because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}