using OrderService.Application.Features.Commands;
using Shared.Events;
using Shared.Events.Orders;

namespace OrderService.Application.Services;

public interface IOrderEventService
{
    Task HandleOrderCompletedAsync(OrderCompletedEvent orderCompletedEvent, CancellationToken cancellationToken = default);
    Task HandleOrderFaileddAsync(OrderFailedEvent orderFailedEvent, CancellationToken cancellationToken = default);
    Task HandlerOrderCreatedCommandAsync(OrderCreatedCommandEvent createdCommandEvent, CancellationToken cancellationToken = default);
}