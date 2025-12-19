using System.Collections.Generic;
using MassTransit;
using Shared.Messages;

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
    public List<OrderItemMessage> OrderItemMessages { get; set; } = new();
}

