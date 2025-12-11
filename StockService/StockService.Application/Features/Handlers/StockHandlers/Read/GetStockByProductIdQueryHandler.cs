using MediatR;
using StockService.Application.Features.Queries.StockQueries;
using StockService.Application.Features.Results.StockResults;
using StockService.Application.Repositories;
using StockService.Domain.Entities;

namespace StockService.Application.Features.Handlers.StockHandlers.Read;

public class GetStockByProductIdQueryHandler : IRequestHandler<GetStockByProductIdQuery, GetStockByProductIdQueryResult>
{
    private readonly IGenericRepository<Stock> _stockRepository;

    public GetStockByProductIdQueryHandler(IGenericRepository<Stock> stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<GetStockByProductIdQueryResult?> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
    {
        var stocks = await _stockRepository.GetAllAsync(s => s.ProductId == request.ProductId, cancellationToken);
        var stock = stocks.FirstOrDefault();
        
        if (stock == null)
            return null;

        return new GetStockByProductIdQueryResult
        {
            ProductId = stock.ProductId,
            Count = stock.Count
        };
    }
}
