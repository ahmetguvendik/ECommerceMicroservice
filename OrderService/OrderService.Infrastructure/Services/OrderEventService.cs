using MassTransit.Mediator;
using OrderService.Application.Features.Commands;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Contexts;
using Shared.Events;
using Shared.Events.Orders;
using Shared.Messages;

namespace OrderService.Infrastructure.Services;

public class OrderEventService : IOrderEventService
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly IMediator _mediator;

    public OrderEventService(OrderServiceDbContext dbContext, IMediator mediator)
    {
         _dbContext = dbContext;
         _mediator = mediator;
    }
    
    public async Task HandleOrderCompletedAsync(OrderCompletedEvent orderCompletedEvent, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.FindAsync(orderCompletedEvent.OrderId);
        if (order != null)
        {
            order.Statues = OrderStatues.Completed;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandleOrderFaileddAsync(OrderFailedEvent orderFailedEvent, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.FindAsync(orderFailedEvent.OrderId);
        if (order != null)
        {
            order.Statues = OrderStatues.Fail;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task HandlerOrderCreatedCommandAsync(OrderCreatedCommandEvent createdCommandEvent, CancellationToken cancellationToken = default)
    {
        var orderId = createdCommandEvent.OrderId != Guid.Empty ? createdCommandEvent.OrderId : Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            CorrelationId = createdCommandEvent.CorrelationId,
            OrderId = orderId,
            CustomerId = createdCommandEvent.CustomerId,
            TotalAmount = createdCommandEvent.TotalAmount,
            OrderItemList = (createdCommandEvent.Items ?? new List<OrderItemMessage>()).Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await _mediator.Send(command, cancellationToken);
    }

  
}