using MediatR;
using OrderService.Application.Features.Commands;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Events;
using System;
using System.Linq;

namespace OrderService.Infrastructure.Services;

public class OrderEventService : IOrderEventService
{
    private readonly IMediator _mediator;

    public OrderEventService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleOrderStartedAsync(OrderStartedEvent orderStartedEvent, CancellationToken cancellationToken = default)
    {
        if (orderStartedEvent.Items == null || !orderStartedEvent.Items.Any())
        {
            throw new InvalidOperationException("OrderStartedEvent does not contain order items.");
        }

        try
        {
            var command = new CreateOrderCommand
            {
                OrderId = orderStartedEvent.OrderId != Guid.Empty ? orderStartedEvent.OrderId : Guid.NewGuid(),
                CustomerId = orderStartedEvent.CustomerId,
                TotalAmount = orderStartedEvent.TotalAmount,
                Statues = OrderStatues.Suspend,
                OrderItemList = orderStartedEvent.Items.Select(item => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = Guid.Empty,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            await _mediator.Send(command, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //Burada hata olduguna dair islemleri yap
            throw;
        }
       
    }
}