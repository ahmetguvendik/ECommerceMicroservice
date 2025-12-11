using MediatR;
using BasketService.Application.Features.Results.BasketResults;

namespace BasketService.Application.Features.Queries.BasketQueries;

public class GetBasketByIdQuery : IRequest<GetBasketByIdQueryResult?>
{
    public Guid Id { get; set; }

    public GetBasketByIdQuery(Guid id)
    {
        Id = id;
    }
}
