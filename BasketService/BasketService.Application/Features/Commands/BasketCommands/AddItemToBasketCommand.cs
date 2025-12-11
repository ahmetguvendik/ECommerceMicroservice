using MediatR;

namespace BasketService.Application.Features.Commands.BasketCommands;

public class AddItemToBasketCommand : IRequest
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

