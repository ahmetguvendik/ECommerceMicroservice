using MassTransit;
using OrderService.Application.Services;
using Shared.Events.Orders;

namespace OrderService.Infrastructure.Consumers;

public class OrderCompletedEventConsumer: IConsumer<OrderCompletedEvent>
{
    private readonly IOrderEventService _orderEventService;

    public OrderCompletedEventConsumer(IOrderEventService orderEventService)
    {
         _orderEventService = orderEventService;
    }
    
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var @event = context.Message;
        await _orderEventService.HandleOrderCompletedAsync(@event, context.CancellationToken);
    }
}