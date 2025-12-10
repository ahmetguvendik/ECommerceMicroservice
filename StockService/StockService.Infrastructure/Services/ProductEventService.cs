using MassTransit;
using Microsoft.Extensions.Logging;
using Shared;
using StockService.Application.Repositories;
using StockService.Application.Services;
using StockService.Application.UnitOfWorks;
using StockService.Domain.Entities;
using Shared.Events;

namespace StockService.Infrastructure.Services;


public class ProductEventService : IProductEventService
{
    private readonly IGenericRepository<Stock> _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    
    public ProductEventService(IGenericRepository<Stock> stockRepository, IUnitOfWork unitOfWork, ISendEndpointProvider sendEndpointProvider)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _sendEndpointProvider = sendEndpointProvider;
 
    }

    public async Task HandleProductCreatedAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken = default)
    {

        try
        {
            // Yeni ürün için stok kaydı oluştur
            var existingStock = await _stockRepository.GetByIdAsync(productCreatedEvent.ProdcutId, cancellationToken);
            
            if (existingStock != null)
            {
                return;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var newStock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = productCreatedEvent.ProdcutId,
                Count = productCreatedEvent.InitialStockCount // Product eklerken belirlenen başlangıç stoku
            };

            await _stockRepository.CreateAsync(newStock, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken); 

        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            //basarisiz olursa tekrardan product a bildirmek gerekiyor!!
           var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.Product_StockCreationFailedEvent}"));
           StockCreationFailedEvent creationFailedEvent = new StockCreationFailedEvent();
           creationFailedEvent.ProductId = productCreatedEvent.ProdcutId;
           creationFailedEvent.ErrorMessage = ex.Message;
           creationFailedEvent.FailedAt = DateTime.UtcNow;
           await sendEndpoint.Send(creationFailedEvent, cancellationToken);
            throw;
        }
    }
}
