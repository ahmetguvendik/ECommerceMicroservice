using Shared.Messages;

namespace Shared.Events.Orders;

public class OrderStartedEvent
{
    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemMessage> Items { get; set; }
}

