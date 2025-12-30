using Shared.Events;

namespace StockService.Application.Services;

public interface IProductEventService
{
    Task HandleProductCreatedAsync(ProductCreatedEvent productCreatedEvent, Guid? messageId = null, CancellationToken cancellationToken = default);
    Task HandleProductUpdatedAsync(ProductUpdatedEvent productUpdatedEvent, Guid? messageId = null, CancellationToken cancellationToken = default);
    Task HandleProductDeletedAsync(ProductDeletedEvent productDeletedEvent, Guid? messageId = null, CancellationToken cancellationToken = default);
}
