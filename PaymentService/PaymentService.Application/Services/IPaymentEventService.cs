using Shared.Events.Payments;

namespace PaymentService.Application.Services;

public interface IPaymentEventService
{
    Task HandlePaymentStartedAsync(PaymentStartedEvent paymentStartedEvent, CancellationToken cancellationToken = default);
}

