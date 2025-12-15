using System;
using System.Collections.Generic;

namespace Shared.Messages;

public class CreateOrderCommandMessage
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemMessage> Items { get; set; } = new();
}

