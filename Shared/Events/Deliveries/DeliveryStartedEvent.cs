using MassTransit;
using Shared.Messages;

namespace Shared.Events.Deliveries;

public class DeliveryStartedEvent : CorrelatedBy<Guid>
{
    public DeliveryStartedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public Guid OrderId { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; } = new();
}

