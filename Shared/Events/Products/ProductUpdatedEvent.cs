namespace Shared.Events;

public class ProductUpdatedEvent
{
    public Guid Id { get; set; }
    public int? StockCount { get; set; }
    public Guid IdempotentToken { get; set; }
}