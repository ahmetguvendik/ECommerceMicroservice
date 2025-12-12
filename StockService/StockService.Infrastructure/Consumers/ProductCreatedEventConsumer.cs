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
        // Application katmanındaki service'e yönlendir
        await _productEventService.HandleProductCreatedAsync(@event, context.CancellationToken);
    }
}
