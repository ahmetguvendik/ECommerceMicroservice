using MassTransit;
using MediatR;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Repositories;
using ProductService.Application.UnitOfWorks;
using ProductService.Domain.Entities;
using Shared.Events;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<ProductCategory> _productCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<ProductCategory> productCategoryRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var category = await _productCategoryRepository.GetByIdAsync(request.ProductCategoryId, cancellationToken);
            if (category == null)
                throw new ArgumentException($"ProductCategory {request.ProductCategoryId} not found");

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
                CreatedTime = DateTime.UtcNow
            };

            await _productRepository.CreateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // EVENT
            var productCreatedEvent = new ProductCreatedEvent
            {
                ProdcutId = product.Id,
                InitialStockCount = request.InitialStockCount,
                Sku = product.Sku,
                Name = product.Name
            };

            await _publishEndpoint.Publish(productCreatedEvent, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
