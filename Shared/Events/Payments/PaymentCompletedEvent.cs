using MassTransit;

namespace Shared.Events.Payments;

public class PaymentCompletedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; }
    public PaymentCompletedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
}