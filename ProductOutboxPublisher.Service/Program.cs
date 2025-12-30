using MassTransit;
using ProductOutboxPublisher.Service.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

// Connection string: prefer appsettings connection string, fallback env
var productOutboxConn = builder.Configuration.GetConnectionString("ProductOutboxConnection")
                         ?? builder.Configuration.GetConnectionString("DefaultConnection")
                         ?? Environment.GetEnvironmentVariable("ProductOutboxConnection")
                         ?? Environment.GetEnvironmentVariable("DefaultConnection");
if (string.IsNullOrWhiteSpace(productOutboxConn))
{
    throw new InvalidOperationException("ProductOutboxConnection/DefaultConnection not found in configuration or environment.");
}
ProductOutboxPublisher.Service.ProductOutboxSingletonDatabase.Initialize(productOutboxConn);

// MassTransit / RabbitMQ
var rabbitMqConn = builder.Configuration.GetConnectionString("RabbitMq")
                   ?? Environment.GetEnvironmentVariable("RabbitMq");
if (string.IsNullOrWhiteSpace(rabbitMqConn))
{
    throw new InvalidOperationException("RabbitMq connection string not found in configuration or environment.");
}

builder.Services.AddMassTransit(cfg =>
{
    cfg.UsingRabbitMq((context, busCfg) =>
    {
        busCfg.Host(rabbitMqConn);
    });
});
builder.Services.AddQuartz(options =>
{
    JobKey jobKey = JobKey.Create("ProductOutboxPublisherJob");
    options.AddJob<ProductOutboxPublishJob>(opt => opt.WithIdentity(jobKey));
    TriggerKey triggerKey = new TriggerKey("ProductOutboxPublisherTrigger");
    options.AddTrigger(opt => opt.ForJob(jobKey)
        .WithIdentity(triggerKey)
        .StartAt(DateTime.UtcNow)
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();