using System.Text.Json;
using MassTransit;
using MediatR;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Repositories;
using ProductService.Application.UnitOfWorks;
using ProductService.Domain.Entities;
using Shared;
using Shared.Events;

namespace ProductService.Application.Features.Handlers.ProductHandlers.Write;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<ProductCategory> _productCategoryRepository;
    private readonly IProductOutboxRepository _productOutboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<ProductCategory> productCategoryRepository,
        IProductOutboxRepository productOutboxRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productOutboxRepository = productOutboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Ürün var mı?
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new ArgumentException($"Product with Id {request.Id} not found.");

            // Kategori var mı?
            var category = await _productCategoryRepository.GetByIdAsync(request.ProductCategoryId, cancellationToken);
            if (category == null)
                throw new ArgumentException($"ProductCategory with Id {request.ProductCategoryId} not found.");

            // Güncelleme
            product.Name = request.Name;
            product.Description = request.Description;
            product.Barcode = request.Barcode;
            product.Price = request.Price;
            product.Sku = request.Sku;
            product.IsActive = request.IsActive;
            product.ProductCategoryId = request.ProductCategoryId;

            await _productRepository.UpdateAsync(product, cancellationToken);
            // EVENT: ProductUpdatedEvent → Outbox
            if (request.InitialStockCount.HasValue)
            {
                var productUpdatedEvent = new ProductUpdatedEvent
                {
                    Id = product.Id,
                    StockCount = request.InitialStockCount.Value,
                    IdempotentToken = Guid.NewGuid()
                };

                var productOutbox = new ProductOutbox
                {
                    IdempotentToken = productUpdatedEvent.IdempotentToken,
                    OccuredOn = DateTime.UtcNow,
                    ProcessedDate = null,
                    Type = nameof(ProductUpdatedEvent),
                    Payload = JsonSerializer.Serialize(productUpdatedEvent)
                };

                await _productOutboxRepository.CreateAsync(productOutbox, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
