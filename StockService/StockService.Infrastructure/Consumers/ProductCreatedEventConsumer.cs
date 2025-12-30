using MassTransit;
using StockService.Application.Services;
using Shared.Events;

namespace StockService.Infrastructure.Consumers;

public class ProductCreatedEventConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly IProductEventService _productEventService;

    public ProductCreatedEventConsumer(IProductEventService productEventService)
    {
        _productEventService = productEventService;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var @event = context.Message;
        var messageId = context.MessageId ?? Guid.NewGuid();
        // Application katmanındaki service'e yönlendir (inbox/idempotent)
        await _productEventService.HandleProductCreatedAsync(@event, messageId, context.CancellationToken);
    }
}
