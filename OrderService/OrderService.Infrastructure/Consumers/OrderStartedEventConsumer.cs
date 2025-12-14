using MassTransit;
using OrderService.Application.Services;
using Shared.Events;
using Shared.Events.Orders;

namespace OrderService.Infrastructure.Consumers;

public class OrderStartedEventConsumer : IConsumer<OrderStartedEvent>
{
    private readonly IOrderEventService _orderEventService;

    public OrderStartedEventConsumer(IOrderEventService orderEventService)
    {
        _orderEventService = orderEventService;
    }

    public async Task Consume(ConsumeContext<OrderStartedEvent> context)
    {
        var @event = context.Message;
        await _orderEventService.HandleOrderStartedAsync(@event, context.CancellationToken);
    }
}
