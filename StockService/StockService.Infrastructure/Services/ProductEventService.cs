using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;
using StockService.Application.Services;
using StockService.Application.UnitOfWorks;
using StockService.Domain.Entities;
using Shared.Events;

namespace StockService.Infrastructure.Services;

/// <summary>
/// Product event'leri için business logic service implementation
/// Infrastructure katmanında implement edilir
/// </summary>
public class ProductEventService : IProductEventService
{
    private readonly IGenericRepository<Stock> _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductEventService> _logger;

    public ProductEventService(
        IGenericRepository<Stock> stockRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProductEventService> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleProductCreatedAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ProductCreatedEvent. ProductId: {ProductId}, CategoryId: {CategoryId}", 
            productCreatedEvent.ProdcutId, productCreatedEvent.ProductCategoryId);

        try
        {
            // Yeni ürün için stok kaydı oluştur
            var existingStock = await _stockRepository.GetByIdAsync(productCreatedEvent.ProdcutId, cancellationToken);
            
            if (existingStock != null)
            {
                _logger.LogWarning("Stock already exists for ProductId: {ProductId}", productCreatedEvent.ProdcutId);
                return;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var newStock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = productCreatedEvent.ProdcutId,
                Count = 0 // Yeni ürün için başlangıç stoku 0
            };

            await _stockRepository.CreateAsync(newStock, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Stock created successfully for ProductId: {ProductId}", productCreatedEvent.ProdcutId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error handling ProductCreatedEvent for ProductId: {ProductId}", productCreatedEvent.ProdcutId);
            throw;
        }
    }
}
