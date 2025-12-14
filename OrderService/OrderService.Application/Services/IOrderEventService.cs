using Shared.Events;
using Shared.Events.Orders;

namespace OrderService.Application.Services;

public interface IOrderEventService
{
    Task HandleOrderStartedAsync(OrderStartedEvent orderStartedEvent, CancellationToken cancellationToken = default);

}   