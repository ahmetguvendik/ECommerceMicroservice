namespace StockService.Application.Features.Results.StockResults;

public class GetStockByProductIdQueryResult
{
    public Guid ProductId { get; set; }
    public int Count { get; set; }
}