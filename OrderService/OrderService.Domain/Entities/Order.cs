using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } 
}