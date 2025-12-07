using MediatR;
using BasketService.Application.Features.Commands.BasketCommands;
using BasketService.Application.Repositories;
using BasketService.Application.UnitOfWorks;
using BasketService.Domain.Entities;

namespace BasketService.Application.Features.Handlers.BasketHandlers.Write;

public class CreateBasketCommandHandler : IRequestHandler<CreateBasketCommand>
{
    private readonly IGenericRepository<Basket> _basketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBasketCommandHandler(
        IGenericRepository<Basket> basketRepository,
        IUnitOfWork unitOfWork)
    {
        _basketRepository = basketRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(CreateBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            var basket = new Basket
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId
            };
            
            await _basketRepository.CreateAsync(basket, cancellationToken);
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

