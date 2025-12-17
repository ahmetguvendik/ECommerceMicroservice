using DeliveryService.Domain.Enums;

namespace DeliveryService.Domain.Entities;

public class Delivery : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid CorrelationId { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? CompletedAt { get; set; }
}

