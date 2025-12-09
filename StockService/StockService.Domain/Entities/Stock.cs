namespace StockService.Domain.Entities;

public class Stock : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Count { get; set; }
}