using System;
using System.Collections.Generic;
using Shared.Messages;

namespace Shared.Events;

public class OrderStartedEvent
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemMessage> Items { get; set; } = new();
}

