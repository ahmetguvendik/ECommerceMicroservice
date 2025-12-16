using MassTransit;
using OrderService.Application.Services;
using Shared.Events.Orders;

namespace OrderService.Infrastructure.Consumers;

public class CreateOrderCommandConsumer : IConsumer<OrderCreatedCommandEvent>
{
    private readonly IOrderEventService _orderEventService;

    public CreateOrderCommandConsumer(IOrderEventService orderEventService)
    {
        _orderEventService = orderEventService;
    }

    public async Task Consume(ConsumeContext<OrderCreatedCommandEvent> context)
    {
        var @event = context.Message;
        await _orderEventService.HandlerOrderCreatedCommandAsync(@event, context.CancellationToken);
    }
}

