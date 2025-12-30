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

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IGenericRepository<Product>  _productRepository;
    private readonly IProductOutboxRepository _productOutboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public DeleteProductCommandHandler(
        IGenericRepository<Product> productRepository,
        IProductOutboxRepository productOutboxRepository,
        IUnitOfWork unitOfWork,
        ISendEndpointProvider sendEndpointProvider)
    {
         _productRepository = productRepository;
         _productOutboxRepository = productOutboxRepository;
         _unitOfWork = unitOfWork;
         _sendEndpointProvider = sendEndpointProvider;
    }
    
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new Exception($"Product with id {request.Id} not found");
            }
        
            await _productRepository.DeleteAsync(product,cancellationToken);
            
            // Event → Outbox
            var productDeletedEvent = new ProductDeletedEvent
            {
                Id = product.Id,
                IdempotentToken = Guid.NewGuid()
            };

            var productOutbox = new ProductOutbox
            {
                IdempotentToken = productDeletedEvent.IdempotentToken,
                OccuredOn = DateTime.UtcNow,
                ProcessedDate = null,
                Type = nameof(ProductDeletedEvent),
                Payload = JsonSerializer.Serialize(productDeletedEvent)
            };

            await _productOutboxRepository.CreateAsync(productOutbox, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new Exception($"Something went wrong: {e.Message}");
        }
      
        
        
    }
}