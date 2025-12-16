using Shared.Events;

namespace StockService.Application.Services;

public interface IOrderEventService
{
    Task HandleOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default);
}