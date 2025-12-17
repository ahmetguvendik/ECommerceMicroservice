using Shared.Events.Deliveries;

namespace DeliveryService.Application.Services;

public interface IDeliveryEventService
{
    Task HandleDeliveryStartedAsync(DeliveryStartedEvent deliveryStartedEvent, CancellationToken cancellationToken = default);
}

