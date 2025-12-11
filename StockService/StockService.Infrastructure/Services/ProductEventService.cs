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
            // Stock entity'sinde Id (kendi ID'si) ve ProductId (ürün ID'si) var
            // ProductId ile arama yapmalıyız
            var existingStocks = await _stockRepository.GetAllAsync(s => s.ProductId == productCreatedEvent.ProdcutId, cancellationToken);
            var existingStock = existingStocks.FirstOrDefault();
            
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

    public async Task HandleProductUpdatedAsync(ProductUpdatedEvent productUpdatedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ürün güncellendiğinde, stok kaydını bul ve stok sayısını güncelle
            // Stock entity'sinde Id (kendi ID'si) ve ProductId (ürün ID'si) var
            // ProductId ile arama yapmalıyız
            var stocks = await _stockRepository.GetAllAsync(s => s.ProductId == productUpdatedEvent.Id, cancellationToken);
            var stock = stocks.FirstOrDefault();
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            if (stock == null)
            {
                // Stok kaydı yoksa oluştur (normalde bu durum olmamalı, ama güvenlik için)
                throw new Exception("Stock not found");
            }
            else
            {
                // Stok kaydı varsa, stok sayısını güncelle
                stock.Count = productUpdatedEvent.StockCount;
                await _stockRepository.UpdateAsync(stock, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            // Hata durumunda loglama yapılabilir
            throw;
        }
    }

    public async Task HandleProductDeletedAsync(ProductDeletedEvent productDeletedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var stocks =
                await _stockRepository.GetAllAsync(x => x.ProductId == productDeletedEvent.Id, cancellationToken);
            var stock = stocks.FirstOrDefault();
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            if (stock == null)
            {
                throw new Exception("Stock not found");
            }
            else
            {
                await _stockRepository.DeleteAsync(stock, cancellationToken);

            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new Exception("Error", e);
        }
    }
}
