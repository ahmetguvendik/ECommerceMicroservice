using MassTransit;
using Shared.Messages;

namespace Shared.Events.Payments;

public class PaymentStartedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; }
    
    public PaymentStartedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; }   
}