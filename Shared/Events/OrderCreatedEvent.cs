using OrderService.Domain.Enums;
using Shared.Messages;

namespace Shared.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; }
    public OrderStatues OrderStatues { get; set; }
    public DateTime OrderDate { get; set; }

}