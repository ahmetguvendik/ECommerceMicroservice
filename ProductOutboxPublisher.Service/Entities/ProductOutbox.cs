namespace ProductOutboxPublisher.Service.Entities;

public class ProductOutbox
{
    public Guid Id { get; set; }
    public DateTime OccuredOn { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
}