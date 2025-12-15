using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using MediatR;
using OrderService.Application.Features.Commands;
using OrderService.Domain.Entities;
using Shared.Messages;

namespace OrderService.Infrastructure.Consumers;

public class CreateOrderCommandConsumer : IConsumer<CreateOrderCommandMessage>
{
    private readonly IMediator _mediator;

    public CreateOrderCommandConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommandMessage> context)
    {
        var payload = context.Message;
        var orderId = payload.OrderId != Guid.Empty ? payload.OrderId : Guid.NewGuid();

        var command = new CreateOrderCommand
        {
            CorrelationId = payload.CorrelationId,
            OrderId = orderId,
            CustomerId = payload.CustomerId,
            TotalAmount = payload.TotalAmount,
            OrderItemList = (payload.Items ?? new List<OrderItemMessage>()).Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await _mediator.Send(command, context.CancellationToken);
    }
}

