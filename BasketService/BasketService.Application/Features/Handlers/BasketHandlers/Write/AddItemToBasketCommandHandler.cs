using MediatR;
using BasketService.Application.Features.Commands.BasketCommands;
using BasketService.Application.Repositories;
using BasketService.Application.UnitOfWorks;
using BasketService.Domain.Entities;

namespace BasketService.Application.Features.Handlers.BasketHandlers.Write;

public class AddItemToBasketCommandHandler : IRequestHandler<AddItemToBasketCommand>
{
    private readonly IGenericRepository<Basket> _basketRepository;
    private readonly IGenericRepository<BasketItem> _basketItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddItemToBasketCommandHandler(
        IGenericRepository<Basket> basketRepository,
        IGenericRepository<BasketItem> basketItemRepository,
        IUnitOfWork unitOfWork)
    {
        _basketRepository = basketRepository;
        _basketItemRepository = basketItemRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var basket = await _basketRepository.GetByIdAsync(request.BasketId, cancellationToken);
            if (basket == null)
            {
                throw new ArgumentException($"Basket with Id {request.BasketId} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            var basketItem = new BasketItem
            {
                Id = Guid.NewGuid(),
                BasketId = request.BasketId,
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice
            };
            
            await _basketItemRepository.CreateAsync(basketItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

