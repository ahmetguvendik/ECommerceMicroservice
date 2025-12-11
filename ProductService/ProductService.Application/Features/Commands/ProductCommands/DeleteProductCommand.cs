using MediatR;

namespace ProductService.Application.Features.Commands.ProductCommands;

public class DeleteProductCommand : IRequest
{
    public Guid Id { get; set; }
}