namespace StockService.Application.Features.Results.StockResults;

public class GetStockByIdQueryResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Count { get; set; }
}