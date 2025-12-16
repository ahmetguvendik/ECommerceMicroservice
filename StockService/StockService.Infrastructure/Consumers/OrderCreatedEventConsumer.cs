using MassTransit;
using Shared.Events;
using StockService.Application.Services;

namespace StockService.Infrastructure.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IOrderEventService _orderEventService;

    public OrderCreatedEventConsumer(IOrderEventService orderEventService)
    {
         _orderEventService = orderEventService;
    }
    
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var @event = context.Message;
        // Application katmanındaki service'e yönlendir
        await _orderEventService.HandleOrderCreatedAsync(@event, context.CancellationToken);
    }
}