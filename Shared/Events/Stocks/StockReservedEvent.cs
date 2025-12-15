using MassTransit;
using Shared.Messages;

namespace Shared.Events.Stocks;

public class StockReservedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; }
    public List<OrderItemMessage> OrderItemMessages { get; set; }

    public StockReservedEvent(Guid correlationId)
    {
         CorrelationId = correlationId;
    }
}