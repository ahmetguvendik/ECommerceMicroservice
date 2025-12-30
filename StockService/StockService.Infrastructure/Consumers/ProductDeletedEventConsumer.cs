using MassTransit;
using Shared.Events;
using StockService.Application.Services;

namespace StockService.Infrastructure.Consumers;

public class ProductDeletedEventConsumer : IConsumer<ProductDeletedEvent>
{
    private readonly IProductEventService _productEventService;

    public ProductDeletedEventConsumer(IProductEventService productEventService)
    {
         _productEventService = productEventService;
    }
    public Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var  @event = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();
        return _productEventService.HandleProductDeletedAsync(@event, messageId, context.CancellationToken);
    }
}