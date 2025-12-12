using MediatR;

namespace BasketService.Application.Features.Commands.BasketCommands;

public class CheckoutBasketCommand : IRequest<Guid>
{
    public Guid CustomerId { get; set; }
}
