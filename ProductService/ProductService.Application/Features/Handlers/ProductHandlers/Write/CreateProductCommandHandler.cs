using MassTransit;
using MediatR;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Repositories;
using ProductService.Application.UnitOfWorks;
using ProductService.Domain.Entities;
using Shared.Events;

namespace ProductService.Application.Features.Handlers.ProductHandlers.Write;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
{
    private readonly IGenericRepository<Product>  _productRepository;
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
        try
        {
            // ProductCategory'nin var olup olmadığını kontrol et
            var category = await _productCategoryRepository.GetByIdAsync(request.ProductCategoryId, cancellationToken);
            if (category == null)
            {
                throw new ArgumentException($"ProductCategory with Id {request.ProductCategoryId} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Barcode = request.Barcode,
                ProductCategoryId = request.ProductCategoryId,
                Sku = request.Sku,
                Price = request.Price,
                IsActive = request.IsActive
            };
            
            await _productRepository.CreateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            ProductCreatedEvent productCreatedEvent = new ProductCreatedEvent();
            productCreatedEvent.ProdcutId = product.Id;
            productCreatedEvent.ProductCategoryId = product.ProductCategoryId;
            await _publishEndpoint.Publish(productCreatedEvent, cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
       
    }
}