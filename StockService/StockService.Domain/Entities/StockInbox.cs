namespace StockService.Domain.Entities;

public class StockInbox
{
    public Guid IdempotentToken { get; set; }
    public bool Processed { get; set; }
    public string Payload { get; set; }
}