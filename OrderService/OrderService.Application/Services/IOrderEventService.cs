using Shared.Events;

namespace OrderService.Application.Services;

public interface IOrderEventService
{
    Task HandleOrderStartedAsync(OrderStartedEvent orderStartedEvent, CancellationToken cancellationToken = default);

}   