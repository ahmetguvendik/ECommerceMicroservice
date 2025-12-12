using MassTransit;
using MediatR;
using OrderService.Application.Features.Commands;
using OrderService.Application.Repositories;
using OrderService.Application.UnitOfWorks;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Events;
using Shared.Messages;
using System;
using System.Linq;

namespace OrderService.Application.Features.Handlers.Wrıte;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
{
    private readonly IGenericRepository<Order>  _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;


    public CreateOrderCommandHandler(IGenericRepository<Order> orderRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
    {
         _orderRepository = orderRepository;
         _unitOfWork = unitOfWork;
         _publishEndpoint = publishEndpoint;

    }
    
    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var orderId = request.OrderId != Guid.Empty ? request.OrderId : Guid.NewGuid();

        try
        {
            if (request.OrderItemList == null || !request.OrderItemList.Any())
            {
                throw new ArgumentException("Sipariş en az bir ürün içermelidir.", nameof(request.OrderItemList));
            }

            var orderDate = DateTime.UtcNow;
            var orderItems = request.OrderItemList
                .Select(item => new OrderItem
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                })
                .ToList();

            var computedTotal = orderItems.Sum(item => item.TotalPrice);

            var order = new Order
            {
                Id = orderId,
                CustomerId = request.CustomerId,
                Items = orderItems,
                TotalAmount = request.TotalAmount > 0 ? request.TotalAmount : computedTotal,
                Statues = request.Statues == default ? OrderStatues.Suspend : (OrderStatues)request.Statues,
            };

            await _orderRepository.CreateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                TotalPrice = order.TotalAmount,
                OrderItemMessages = order.Items
                    .Select(item => new OrderItemMessage
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    })
                    .ToList(),
                OrderStatues = order.Statues,
                OrderDate = orderDate
            };

            await _publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            var orderFailedEvent = new OrderFailedEvent();
            orderFailedEvent.OrderId = orderId;
            await _publishEndpoint.Publish(orderFailedEvent, cancellationToken);
            throw new ApplicationException($"An error occurred processing {nameof(CreateOrderCommand)}", e);
        }
    }


}