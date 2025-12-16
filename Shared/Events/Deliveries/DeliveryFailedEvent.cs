using MassTransit;

namespace Shared.Events.Deliveries;

public class DeliveryFailedEvent : CorrelatedBy<Guid>
{
    public DeliveryFailedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; }
    public Guid OrderId { get; set; }
    public string Message { get; set; }
}

