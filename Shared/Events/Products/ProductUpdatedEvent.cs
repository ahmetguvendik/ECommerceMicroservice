namespace Shared.Events;

public class ProductUpdatedEvent
{
    public Guid Id { get; set; }
    public int? StockCount { get; set; }
}