using Shared.Messages;

namespace StockService.Application.Services;

public interface IStockEventService
{
    Task HandleStockRollbackMessage(StockRollbackMessage message, CancellationToken cancellationToken);
}