using MediatR;
using StockService.Application.Features.Results.StockResults;

namespace StockService.Application.Features.Queries.StockQueries;

public class GetStockByProductIdQuery : IRequest<GetStockByProductIdQueryResult>
{
    public Guid ProductId { get; set; }
}

