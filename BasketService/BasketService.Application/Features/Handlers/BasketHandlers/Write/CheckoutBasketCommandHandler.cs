using MassTransit;
using MediatR;
using Shared.Events;
using Shared.Messages;
using BasketService.Application.Features.Commands.BasketCommands;
using BasketService.Application.Repositories;
using BasketService.Domain.Entities;
using System;
using System.Linq;

namespace BasketService.Application.Features.Handlers.BasketHandlers.Write;

public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand, Guid>
{
    private readonly IGenericRepository<Basket> _basketRepository;
    private readonly IGenericRepository<BasketItem> _basketItemRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckoutBasketCommandHandler(
        IGenericRepository<Basket> basketRepository,
        IGenericRepository<BasketItem> basketItemRepository,
        IPublishEndpoint publishEndpoint)
    {
        _basketRepository = basketRepository;
        _basketItemRepository = basketItemRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var baskets = await _basketRepository.GetAllAsync(b => b.CustomerId == request.CustomerId, cancellationToken);
        var basket = baskets.FirstOrDefault();

        if (basket == null)
        {
            throw new ArgumentException($"Basket for CustomerId {request.CustomerId} not found.");
        }

        var basketItems = await _basketItemRepository.GetAllAsync(item => item.BasketId == basket.Id, cancellationToken);
        if (!basketItems.Any())
        {
            throw new InvalidOperationException("Basket is empty.");
        }

        var orderId = Guid.NewGuid();
        var orderStartedEvent = new OrderStartedEvent
        {
            CorrelationId = Guid.NewGuid(),
            OrderId = orderId,
            BasketId = basket.Id,
            CustomerId = basket.CustomerId,
            TotalAmount = basketItems.Sum(item => item.TotalPrice),
            Items = basketItems.Select(item => new OrderItemMessage
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await _publishEndpoint.Publish(orderStartedEvent, cancellationToken);
        return orderId;
    }
}
