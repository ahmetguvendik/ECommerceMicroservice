using MediatR;
using ProductService.Application.Features.Results;

namespace ProductService.Application.Features.Queries.ProductQueries;

public class GetProductByIdQuery : IRequest<GetProductByIdQueryResult?>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}

