using DeliveryService.Application.Services;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Enums;
using DeliveryService.Infrastructure.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events.Deliveries;

namespace DeliveryService.Infrastructure.Services;

public class DeliveryEventService : IDeliveryEventService
{
    private readonly DeliveryServiceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeliveryEventService(DeliveryServiceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task HandleDeliveryStartedAsync(DeliveryStartedEvent deliveryStartedEvent, CancellationToken cancellationToken = default)
    {
        var delivery = await _dbContext.Deliveries
            .FirstOrDefaultAsync(x => x.OrderId == deliveryStartedEvent.OrderId && x.CorrelationId == deliveryStartedEvent.CorrelationId, cancellationToken);

        if (delivery == null)
        {
            delivery = new Delivery
            {
                Id = Guid.NewGuid(),
                OrderId = deliveryStartedEvent.OrderId,
                CorrelationId = deliveryStartedEvent.CorrelationId,
                Status = DeliveryStatus.Pending,
                CreatedTime = DateTime.Now,
            };
            await _dbContext.Deliveries.AddAsync(delivery, cancellationToken);
        }

        try
        {
            // Basit senaryo: her zaman teslimat tamamlandı varsayalım
            delivery.Status = DeliveryStatus.Completed;
            delivery.CompletedAt = DateTime.Now;
            delivery.ModifiedTime = DateTime.Now;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new DeliveryCompletedEvent(delivery.CorrelationId)
            {
                OrderId = delivery.OrderId
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            delivery.Status = DeliveryStatus.Failed;
            delivery.FailureReason = ex.Message;
            delivery.ModifiedTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new DeliveryFailedEvent(delivery.CorrelationId)
            {
                OrderId = delivery.OrderId,
                Message = ex.Message
            }, cancellationToken);

            throw;
        }
    }
}

