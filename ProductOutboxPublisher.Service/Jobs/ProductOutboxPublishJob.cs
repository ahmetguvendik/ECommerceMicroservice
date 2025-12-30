using System.Text.Json;
using MassTransit;
using ProductOutboxPublisher.Service.Entities;
using Quartz;
using Shared.Events;

namespace ProductOutboxPublisher.Service.Jobs;

public class ProductOutboxPublishJob : IJob
{
    private readonly IPublishEndpoint _publishEndpoint;
    public ProductOutboxPublishJob(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Execute(IJobExecutionContext context)
    {
       if (!ProductOutboxSingletonDatabase.DataReaderState)
       {
           return;
       }

       ProductOutboxSingletonDatabase.DataReaderBusy();
       var productOutboxes = await ProductOutboxSingletonDatabase.QueryAsync<ProductOutbox>(
           "SELECT * FROM \"ProductOutboxes\" WHERE \"ProcessedDate\" IS NULL ORDER BY \"OccuredOn\" ASC");
       foreach (var productOutbox in productOutboxes)
       {
           if (productOutbox.Type != nameof(ProductCreatedEvent))
           {
               continue;
           }

           var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(productOutbox.Payload);
           if (productCreatedEvent == null)
           {
               continue;
           }

           await _publishEndpoint.Publish(productCreatedEvent);
           await ProductOutboxSingletonDatabase.ExecuteAsync(
               "UPDATE \"ProductOutboxes\" SET \"ProcessedDate\" = NOW() WHERE \"Id\" = @Id",
               new { productOutbox.Id });
       }

       ProductOutboxSingletonDatabase.DataReaderReady();
       await Console.Out.WriteLineAsync("Product Outbox Publish Job Completed");
    }
}