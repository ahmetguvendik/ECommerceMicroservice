using MediatR;

namespace BasketService.Application.Features.Commands.BasketCommands;

public class CreateBasketCommand : IRequest
{
    public Guid CustomerId { get; set; }
}

