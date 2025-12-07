using MediatR;

namespace BasketService.Application.Features.Commands.BasketCommands;

public class AddItemToBasketCommand : IRequest
{
    public Guid BasketId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

