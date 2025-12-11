using Shared.Events;

namespace StockService.Application.Services;

public interface IProductEventService
{
    Task HandleProductCreatedAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken = default);
    Task HandleProductUpdatedAsync(ProductUpdatedEvent productUpdatedEvent, CancellationToken cancellationToken = default);
    Task HandleProductDeletedAsync(ProductDeletedEvent productDeletedEvent, CancellationToken cancellationToken = default);
}
