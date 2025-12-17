using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? CompletedAt { get; set; }
}

