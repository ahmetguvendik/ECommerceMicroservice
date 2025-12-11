using MediatR;
using ProductService.Application.Features.Queries.ProductQueries;
using ProductService.Application.Features.Results;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Handlers.ProductHandlers.Read;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdQueryResult?>
{
    private readonly IGenericRepository<Product> _productRepository;

    public GetProductByIdQueryHandler(IGenericRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductByIdQueryResult?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
            return null;

        return new GetProductByIdQueryResult
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Sku = product.Sku,
            Barcode = product.Barcode,
            IsActive = product.IsActive,
            ProductCategoryId = product.ProductCategoryId
        };
    }
}
