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
           switch (productOutbox.Type)
           {
               case nameof(ProductCreatedEvent):
               {
                   var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(productOutbox.Payload);
                   if (productCreatedEvent == null) break;
                   await _publishEndpoint.Publish(productCreatedEvent);
                   break;
               }
               case nameof(ProductUpdatedEvent):
               {
                   var productUpdatedEvent = JsonSerializer.Deserialize<ProductUpdatedEvent>(productOutbox.Payload);
                   if (productUpdatedEvent == null) break;
                   await _publishEndpoint.Publish(productUpdatedEvent);
                   break;
               }
               case nameof(ProductDeletedEvent):
               {
                   var productDeletedEvent = JsonSerializer.Deserialize<ProductDeletedEvent>(productOutbox.Payload);
                   if (productDeletedEvent == null) break;
                   await _publishEndpoint.Publish(productDeletedEvent);
                   break;
               }
               default:
                   break;
           }

           await ProductOutboxSingletonDatabase.ExecuteAsync(
               "UPDATE \"ProductOutboxes\" SET \"ProcessedDate\" = NOW() WHERE \"IdempotentToken\" = @IdempotentToken",
               new { productOutbox.IdempotentToken });
       }

       ProductOutboxSingletonDatabase.DataReaderReady();
       await Console.Out.WriteLineAsync("Product Outbox Publish Job Completed");
    }
}