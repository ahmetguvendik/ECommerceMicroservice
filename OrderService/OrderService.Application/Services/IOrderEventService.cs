using Shared.Events;

namespace OrderService.Application.Services;

public interface IBasketEventService
{
    Task HandleOrderStartedAsync(OrderStartedEvent orderStartedEvent, CancellationToken cancellationToken = default);

}   