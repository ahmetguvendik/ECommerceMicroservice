using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContexts;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;

var builder = Host.CreateApplicationBuilder(args);

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
host.Run();