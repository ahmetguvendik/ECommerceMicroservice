using MassTransit;
using OrderService.Application.Services;
using Shared.Events;

namespace OrderService.Infrastructure.Consumers;

public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
{
    private readonly IOrderEventService _orderEventService;

    public OrderFailedEventConsumer(IOrderEventService orderEventService)
    {
         _orderEventService = orderEventService;
    }
    
    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        var @event = context.Message;
        await _orderEventService.HandleOrderFaileddAsync(@event, context.CancellationToken);
    }
}