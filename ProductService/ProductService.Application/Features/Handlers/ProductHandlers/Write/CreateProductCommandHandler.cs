using System.Text.Json;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Repositories;
using ProductService.Application.UnitOfWorks;
using ProductService.Domain.Entities;
using Shared.Events;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<ProductCategory> _productCategoryRepository;
    private readonly IProductOutboxRepository _productOutboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<ProductCategory> productCategoryRepository,
        IProductOutboxRepository productOutboxRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productOutboxRepository = productOutboxRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handler: Creating product in database - Name: {Name}, SKU: {Sku}", request.Name, request.Sku);
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var category = await _productCategoryRepository.GetByIdAsync(request.ProductCategoryId, cancellationToken);
            if (category == null)
            {
                _logger.LogError("ProductCategory not found: {CategoryId}", request.ProductCategoryId);
                throw new ArgumentException($"ProductCategory {request.ProductCategoryId} not found");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Barcode = request.Barcode,
                Sku = request.Sku,
                Price = request.Price,
                IsActive = request.IsActive,
                ProductCategoryId = request.ProductCategoryId,
            };

            await _productRepository.CreateAsync(product, cancellationToken);
            _logger.LogInformation("Product saved to database: {ProductId}, SKU: {Sku}", product.Id, product.Sku);

            var idempotentToken = Guid.NewGuid();
            // EVENT
            var productCreatedEvent = new ProductCreatedEvent
            {
                ProdcutId = product.Id,
                InitialStockCount = request.InitialStockCount,
                Sku = product.Sku,
                Name = product.Name,
                IdempotentToken = idempotentToken,
            };
            
            #region Outbox Olmadan Direkt Event Gonderme
            //await _publishEndpoint.Publish(productCreatedEvent, cancellationToken);
            //await _unitOfWork.CommitTransactionAsync(cancellationToken);
            #endregion
            
            #region Outbox Ile Islemler

            var productOutbox = new ProductOutbox
            {
                IdempotentToken = idempotentToken,
                OccuredOn = DateTime.UtcNow,
                ProcessedDate = null,
                Type = nameof(ProductCreatedEvent),
                Payload = JsonSerializer.Serialize(productCreatedEvent)
            };

            await _productOutboxRepository.CreateAsync(productOutbox, cancellationToken);
            _logger.LogInformation("ProductCreatedEvent added to outbox - ProductId: {ProductId}, IdempotentToken: {Token}", 
                product.Id, idempotentToken);
            
            #endregion

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            _logger.LogInformation("Product creation completed successfully - ProductId: {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product - Name: {Name}, SKU: {Sku}", request.Name, request.Sku);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
