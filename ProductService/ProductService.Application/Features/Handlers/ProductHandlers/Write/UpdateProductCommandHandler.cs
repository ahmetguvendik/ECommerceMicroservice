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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public UpdateProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IGenericRepository<ProductCategory> productCategoryRepository,
        IUnitOfWork unitOfWork,
        ISendEndpointProvider sendEndpointProvider)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
        _sendEndpointProvider = sendEndpointProvider;
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // EVENT: ProductUpdatedEvent
            var productUpdatedEvent = new ProductUpdatedEvent
            {
                Id = product.Id,
                StockCount = request.InitialStockCount
            };
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Stock_ProductUpdatedEventQueue}"));
            await sendEndpoint.Send(productUpdatedEvent, cancellationToken);
            
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
