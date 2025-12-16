using System;
using System.Linq;
using MassTransit;
using Shared.Events;
using Shared.Events.Stocks;
using StockService.Application.Repositories;
using StockService.Application.Services;
using StockService.Application.UnitOfWorks;
using StockService.Domain.Entities;
using System.Collections.Concurrent;

namespace StockService.Infrastructure.Services;

public class OrderEventService : IOrderEventService
{
    private static readonly ConcurrentDictionary<Guid, byte> ProcessedCorrelationIds = new();
    private readonly IGenericRepository<Stock> _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderEventService(
        IGenericRepository<Stock> stockRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default)
    {
        // Idempotent kontrol: aynı CorrelationId daha önce işlendi ise tekrar stok düşme
        if (!ProcessedCorrelationIds.TryAdd(orderCreatedEvent.CorrelationId, 0))
        {
            return;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderItems = orderCreatedEvent.OrderItemMessages ?? [];
            var productIds = orderItems.Select(x => x.ProductId).ToList();

            var stocks = await _stockRepository.GetAllAsync(s => productIds.Contains(s.ProductId), cancellationToken);

            foreach (var item in orderItems)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);
                if (stock == null || stock.Count < item.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    await _publishEndpoint.Publish(new StockNotReservedEvent(orderCreatedEvent.CorrelationId)
                    {
                        Message = $"Insufficient stock for product {item.ProductId}"
                    }, cancellationToken);
                    return;
                }

                stock.Count -= item.Quantity;
                await _stockRepository.UpdateAsync(stock, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _publishEndpoint.Publish(new StockReservedEvent(orderCreatedEvent.CorrelationId)
            {
                OrderItemMessages = orderItems
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            await _publishEndpoint.Publish(new StockNotReservedEvent(orderCreatedEvent.CorrelationId)
            {
                Message = $"Stock check failed: {ex.Message}"
            }, cancellationToken);
            throw;
        }
    }
} 