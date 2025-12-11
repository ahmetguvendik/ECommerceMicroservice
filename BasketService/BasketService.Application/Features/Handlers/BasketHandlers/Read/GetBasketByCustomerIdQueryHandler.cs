using MediatR;
using BasketService.Application.Features.Queries.BasketQueries;
using BasketService.Application.Features.Results.BasketResults;
using BasketService.Application.Repositories;
using BasketService.Domain.Entities;

namespace BasketService.Application.Features.Handlers.BasketHandlers.Read;

public class GetBasketByCustomerIdQueryHandler : IRequestHandler<GetBasketByCustomerIdQuery, GetBasketByCustomerIdQueryResult?>
{
    private readonly IGenericRepository<Basket> _basketRepository;
    private readonly IGenericRepository<BasketItem> _basketItemRepository;

    public GetBasketByCustomerIdQueryHandler(
        IGenericRepository<Basket> basketRepository,
        IGenericRepository<BasketItem> basketItemRepository)
    {
        _basketRepository = basketRepository;
        _basketItemRepository = basketItemRepository;
    }

    public async Task<GetBasketByCustomerIdQueryResult?> Handle(GetBasketByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        // CustomerId ile sepeti bul
        var baskets = await _basketRepository.GetAllAsync(
            b => b.CustomerId == request.CustomerId, 
            cancellationToken);
        
        var basket = baskets.FirstOrDefault();
        
        if (basket == null)
            return null;

        // BasketItem'ları al
        var basketItems = await _basketItemRepository.GetAllAsync(
            item => item.BasketId == basket.Id, 
            cancellationToken);

        return new GetBasketByCustomerIdQueryResult
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
