using DeliveryService.Application.Services;
using MassTransit;
using Shared.Events.Deliveries;

namespace DeliveryService.Infrastructure.Consumers;

public class DeliveryStartedEventConsumer : IConsumer<DeliveryStartedEvent>
{
    private readonly IDeliveryEventService _deliveryEventService;

    public DeliveryStartedEventConsumer(IDeliveryEventService deliveryEventService)
    {
        _deliveryEventService = deliveryEventService;
    }

    public async Task Consume(ConsumeContext<DeliveryStartedEvent> context)
    {
        await _deliveryEventService.HandleDeliveryStartedAsync(context.Message, context.CancellationToken);
    }
}

