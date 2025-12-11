namespace BasketService.Application.Features.Results.BasketResults;

public class GetBasketByIdQueryResult
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}



