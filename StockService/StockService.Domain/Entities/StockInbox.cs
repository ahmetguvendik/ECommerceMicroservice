namespace StockService.Domain.Entities;

public class StockInbox : BaseEntity
{
    public bool Processed { get; set; }
    public string Payload { get; set; }
}