using BasketService.Application.DTOs;

namespace BasketService.Application.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

