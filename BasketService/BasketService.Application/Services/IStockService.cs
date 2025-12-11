using BasketService.Application.DTOs;

namespace BasketService.Application.Services;

public interface IStockService
{
    Task<StockDto?> GetStockByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}

