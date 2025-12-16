using MassTransit;
using Shared.Messages;
using StockService.Application.Services;

namespace StockService.Infrastructure.Consumers;

public class StockRollbackMessageConsumer : IConsumer<StockRollbackMessage>
{
    private readonly IStockEventService _stockEventService;

    public StockRollbackMessageConsumer(IStockEventService stockEventService)
    {
         _stockEventService = stockEventService;
    }
    
    public async Task Consume(ConsumeContext<StockRollbackMessage> context)
    {
       var @event = context.Message;
       await _stockEventService.HandleStockRollbackMessage(@event, context.CancellationToken);
    }
}