using MassTransit;
using Shared.Events;
using StockService.Application.Services;

namespace StockService.Infrastructure.Consumers;

public class ProductUpdatedEventConsumer : IConsumer<ProductUpdatedEvent>
{
    private readonly IProductEventService _productEventService;

    public ProductUpdatedEventConsumer(IProductEventService productEventService)
    {
        _productEventService = productEventService;
    }
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var @event = context.Message;
        // Application katmanındaki service'e yönlendir
        var messageId = context.MessageId ?? Guid.NewGuid();
        await _productEventService.HandleProductUpdatedAsync(@event, messageId, context.CancellationToken);
    }
}