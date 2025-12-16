using MassTransit;

namespace Shared.Events.Deliveries;

public class DeliveryCompletedEvent : CorrelatedBy<Guid>
{
    public DeliveryCompletedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; }
    public Guid OrderId { get; set; }
}

