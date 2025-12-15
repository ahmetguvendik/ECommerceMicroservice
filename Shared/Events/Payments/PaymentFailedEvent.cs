using MassTransit;
using Shared.Messages;

namespace Shared.Events.Payments;

public class PaymentFailedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; }
    public PaymentFailedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public string Message { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; }   
}