using MediatR;
using StockService.Application.Features.Queries.StockQueries;
using StockService.Application.Features.Results.StockResults;
using StockService.Application.Repositories;
using StockService.Domain.Entities;

namespace StockService.Application.Features.Handlers.StockHandlers.Read;

public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, GetStockByIdQueryResult>
{
    private readonly IGenericRepository<Stock> _stockRepository;

    public GetStockByIdQueryHandler(IGenericRepository<Stock> stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<GetStockByIdQueryResult> Handle(GetStockByIdQuery request, CancellationToken cancellationToken)
    {
        var stock = await _stockRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (stock == null)
            return null;

        return new GetStockByIdQueryResult
        {
            Id = stock.Id,
            ProductId = stock.ProductId,
            Count = stock.Count
        };
    }
}
