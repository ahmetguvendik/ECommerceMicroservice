using MediatR;

namespace ProductService.Application.Features.Commands.ProductCommands;

public class CreateProductCommand : IRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Sku { get; set; }       
    public string Barcode { get; set; }  

    public bool IsActive { get; set; } = true;
    public Guid ProductCategoryId { get; set; }
    public int InitialStockCount { get; set; } = 0;
}