using MassTransit;
using Shared.Messages;

namespace Shared.Events.Stocks;

public class StockNotReservedEvent: CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; }
    public string Message { get; set; }
    public StockNotReservedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    
}