using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using ProductOutboxPublisher.Service.Entities;
using Quartz;
using Shared.Events;
using Shared.Extensions;

namespace ProductOutboxPublisher.Service.Jobs;

public class ProductOutboxPublishJob : IJob
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductOutboxPublishJob> _logger;
    
    public ProductOutboxPublishJob(IPublishEndpoint publishEndpoint, ILogger<ProductOutboxPublishJob> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
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
           // Use IdempotentToken as CorrelationId for traceability
           var correlationId = productOutbox.IdempotentToken;
           CorrelationIdExtensions.SetCorrelationIdToNLog(correlationId.ToString());
           
           try
           {
               switch (productOutbox.Type)
               {
                   case nameof(ProductCreatedEvent):
                   {
                       var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(productOutbox.Payload);
                       if (productCreatedEvent == null) break;
                       
                       // Publish with CorrelationId
                       await _publishEndpoint.Publish(productCreatedEvent, ctx => 
                       {
                           ctx.CorrelationId = correlationId;
                       });
                       
                       _logger.LogInformation("Published ProductCreatedEvent - ProductId: {ProductId}, CorrelationId: {CorrelationId}",
                           productCreatedEvent.ProdcutId, correlationId);
                       break;
                   }
                   case nameof(ProductUpdatedEvent):
                   {
                       var productUpdatedEvent = JsonSerializer.Deserialize<ProductUpdatedEvent>(productOutbox.Payload);
                       if (productUpdatedEvent == null) break;
                       
                       // Publish with CorrelationId
                       await _publishEndpoint.Publish(productUpdatedEvent, ctx => 
                       {
                           ctx.CorrelationId = correlationId;
                       });
                       
                       _logger.LogInformation("Published ProductUpdatedEvent - ProductId: {ProductId}, CorrelationId: {CorrelationId}",
                           productUpdatedEvent.Id, correlationId);
                       break;
                   }
                   case nameof(ProductDeletedEvent):
                   {
                       var productDeletedEvent = JsonSerializer.Deserialize<ProductDeletedEvent>(productOutbox.Payload);
                       if (productDeletedEvent == null) break;
                       
                       // Publish with CorrelationId
                       await _publishEndpoint.Publish(productDeletedEvent, ctx => 
                       {
                           ctx.CorrelationId = correlationId;
                       });
                       
                       _logger.LogInformation("Published ProductDeletedEvent - ProductId: {ProductId}, CorrelationId: {CorrelationId}",
                           productDeletedEvent.Id, correlationId);
                       break;
                   }
                   default:
                       break;
               }
           }
           finally
           {
               CorrelationIdExtensions.ClearCorrelationIdFromNLog();
           }

           await ProductOutboxSingletonDatabase.ExecuteAsync(
               "UPDATE \"ProductOutboxes\" SET \"ProcessedDate\" = NOW() WHERE \"IdempotentToken\" = @IdempotentToken",
               new { productOutbox.IdempotentToken });
       }

       ProductOutboxSingletonDatabase.DataReaderReady();
       
       if (productOutboxes.Any())
       {
           _logger.LogInformation("Product Outbox Publish Job Completed - Published {Count} events", productOutboxes.Count());
       }
    }
}