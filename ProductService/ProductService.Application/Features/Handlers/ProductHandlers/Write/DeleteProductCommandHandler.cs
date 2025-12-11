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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public DeleteProductCommandHandler(IGenericRepository<Product> productRepository, IUnitOfWork unitOfWork, ISendEndpointProvider sendEndpointProvider)
    {
         _productRepository = productRepository;
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            //Event yollanacak stock tarafina 
            var productDeletedEvent = new ProductDeletedEvent();
            productDeletedEvent.Id = product.Id;

            var sendEnpoint =
               await _sendEndpointProvider.GetSendEndpoint(
                    new Uri($"queue:{RabbitMqSettings.Stock_ProductDeletedEventQueue}"));
            await sendEnpoint.Send(productDeletedEvent, cancellationToken);

        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new Exception($"Something went wrong: {e.Message}");
        }
      
        
        
    }
}