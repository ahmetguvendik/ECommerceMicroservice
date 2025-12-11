using MediatR;
using BasketService.Application.Features.Results.BasketResults;

namespace BasketService.Application.Features.Queries.BasketQueries;

public class GetBasketByCustomerIdQuery : IRequest<GetBasketByCustomerIdQueryResult?>
{
    public Guid CustomerId { get; set; }

    public GetBasketByCustomerIdQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}
