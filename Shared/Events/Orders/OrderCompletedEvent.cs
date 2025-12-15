namespace Shared.Events.Orders;

public class OrderCompletedEvent
{
    public Guid OrderId { get; set; }
}