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
        var token = @event.IdempotentToken;
        await _productEventService.HandleProductCreatedAsync(@event, token, context.CancellationToken);
    }
}
