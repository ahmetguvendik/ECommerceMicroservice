using MediatR;
using BasketService.Application.Features.Queries.BasketQueries;
using BasketService.Application.Features.Results.BasketResults;
using BasketService.Application.Repositories;
using BasketService.Domain.Entities;

namespace BasketService.Application.Features.Handlers.BasketHandlers.Read;

public class GetBasketByIdQueryHandler : IRequestHandler<GetBasketByIdQuery, GetBasketByIdQueryResult?>
{
    private readonly IGenericRepository<Basket> _basketRepository;
    private readonly IGenericRepository<BasketItem> _basketItemRepository;

    public GetBasketByIdQueryHandler(
        IGenericRepository<Basket> basketRepository,
        IGenericRepository<BasketItem> basketItemRepository)
    {
        _basketRepository = basketRepository;
        _basketItemRepository = basketItemRepository;
    }

    public async Task<GetBasketByIdQueryResult?> Handle(GetBasketByIdQuery request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (basket == null)
            return null;

        // BasketItem'ları al (Redis'te ayrı saklanıyor)
        var basketItems = await _basketItemRepository.GetAllAsync(
            item => item.BasketId == basket.Id, 
            cancellationToken);

        return new GetBasketByIdQueryResult
        {
            Id = basket.Id,
            CustomerId = basket.CustomerId,
            Items = basketItems.Select(item => new BasketItemDto
            {
                Id = item.Id,
                BasketId = item.BasketId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList(),
            TotalAmount = basketItems.Sum(item => item.TotalPrice)
        };
    }
}
