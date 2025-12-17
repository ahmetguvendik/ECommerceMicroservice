using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Contexts;
using Shared.Events.Payments;

namespace PaymentService.Infrastructure.Services;

public class PaymentEventService : IPaymentEventService
{
    private readonly PaymentServiceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentEventService(PaymentServiceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task HandlePaymentStartedAsync(PaymentStartedEvent paymentStartedEvent, CancellationToken cancellationToken = default)
    {
        // Idempotency: unique index (OrderId, CorrelationId) ensures no duplicates
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(x => x.OrderId == paymentStartedEvent.OrderId && x.CorrelationId == paymentStartedEvent.CorrelationId, cancellationToken);

        if (payment == null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = paymentStartedEvent.OrderId,
                CorrelationId = paymentStartedEvent.CorrelationId,
                Amount = paymentStartedEvent.TotalPrice,
                Status = PaymentStatus.Pending,
                CreatedTime = DateTime.Now, 
            };
            await _dbContext.Payments.AddAsync(payment, cancellationToken);
        }

        try
        {
            // Basit senaryo: her zaman başarılı kabul edelim (gerçek entegrasyonda ödeme sağlayıcısı çağrılır)
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PaymentCompletedEvent(payment.CorrelationId), cancellationToken);
        }
        catch (Exception ex)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = ex.Message;
            payment.ModifiedTime = DateTime.Now;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PaymentFailedEvent(payment.CorrelationId)
            {
                Message = ex.Message,
                OrderItemMessages = paymentStartedEvent.OrderItemMessages ?? new List<Shared.Messages.OrderItemMessage>()
            }, cancellationToken);

            throw;
        }
    }
}

