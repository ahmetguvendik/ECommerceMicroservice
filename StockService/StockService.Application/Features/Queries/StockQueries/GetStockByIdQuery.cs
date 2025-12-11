using MediatR;
using StockService.Application.Features.Results.StockResults;

namespace StockService.Application.Features.Queries.StockQueries;

public class GetStockByIdQuery : IRequest<GetStockByIdQueryResult>
{
    public Guid Id { get; set; }

    public GetStockByIdQuery(Guid id)
    {
        Id = id;
    }
}

