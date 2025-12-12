using MediatR;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Features.Commands;

public class CreateOrderCommand : IRequest
{
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatues Statues { get; set; }
    public List<OrderItem> OrderItemList { get; set; } 
}