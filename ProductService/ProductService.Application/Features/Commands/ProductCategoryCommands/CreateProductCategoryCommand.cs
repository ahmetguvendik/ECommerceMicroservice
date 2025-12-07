using MediatR;

namespace ProductService.Application.Features.Commands.ProductCategoryCommands;

public class CreateProductCategoryCommand : IRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
}