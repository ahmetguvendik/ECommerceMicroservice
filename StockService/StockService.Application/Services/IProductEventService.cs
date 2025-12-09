using Shared.Events;

namespace StockService.Application.Services;

public interface IProductEventService
{
    Task HandleProductCreatedAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken = default);
}
