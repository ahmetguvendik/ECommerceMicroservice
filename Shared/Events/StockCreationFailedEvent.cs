namespace Shared.Events;

public class StockCreationFailedEvent
{
    public Guid ProductId { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime FailedAt { get; set; }
}