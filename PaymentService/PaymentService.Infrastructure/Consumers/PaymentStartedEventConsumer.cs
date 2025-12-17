using MassTransit;
using PaymentService.Application.Services;
using Shared.Events.Payments;

namespace PaymentService.Infrastructure.Consumers;

public class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
{
    private readonly IPaymentEventService _paymentEventService;

    public PaymentStartedEventConsumer(IPaymentEventService paymentEventService)
    {
        _paymentEventService = paymentEventService;
    }

    public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
    {
        await _paymentEventService.HandlePaymentStartedAsync(context.Message, context.CancellationToken);
    }
}

