using System.Text.Json;
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
    private readonly IGenericRepository<StockInbox> _inboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    
    public ProductEventService(
        IGenericRepository<Stock> stockRepository,
        IGenericRepository<StockInbox> inboxRepository,
        IUnitOfWork unitOfWork,
        ISendEndpointProvider sendEndpointProvider)
    {
        _stockRepository = stockRepository;
        _inboxRepository = inboxRepository;
        _unitOfWork = unitOfWork;
        _sendEndpointProvider = sendEndpointProvider;
 
    }

    public async Task HandleProductCreatedAsync(ProductCreatedEvent productCreatedEvent, Guid? messageId = null, CancellationToken cancellationToken = default)
    {

        try
        {
            var inboxId = messageId ?? Guid.NewGuid();
            var existingInbox = await _inboxRepository.GetByIdAsync(inboxId, cancellationToken);
            if (existingInbox != null && existingInbox.Processed)
            {
                return; // idempotent
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Yeni ürün için stok kaydı oluştur
            var existingStocks = await _stockRepository.GetAllAsync(s => s.ProductId == productCreatedEvent.ProdcutId, cancellationToken);
            var existingStock = existingStocks.FirstOrDefault();
            
            if (existingStock != null)
            {
                // yine de inbox'ı processed işaretle
                if (existingInbox == null)
                {
                    var inbox = new StockInbox
                    {
                        Id = inboxId,
                        Processed = true,
                        Payload = JsonSerializer.Serialize(productCreatedEvent)
                    };
                    await _inboxRepository.CreateAsync(inbox, cancellationToken);
                }
                else
                {
                    existingInbox.Processed = true;
                    await _inboxRepository.UpdateAsync(existingInbox, cancellationToken);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return;
            }

            var newStock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = productCreatedEvent.ProdcutId,
                Count = productCreatedEvent.InitialStockCount // Product eklerken belirlenen başlangıç stoku
            };

            await _stockRepository.CreateAsync(newStock, cancellationToken);
            
            if (existingInbox == null)
            {
                var inbox = new StockInbox
                {
                    Id = inboxId,
                    Processed = true,
                    Payload = JsonSerializer.Serialize(productCreatedEvent)
                };
                await _inboxRepository.CreateAsync(inbox, cancellationToken);
            }
            else
            {
                existingInbox.Processed = true;
                await _inboxRepository.UpdateAsync(existingInbox, cancellationToken);
            }
            
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

    public async Task HandleProductUpdatedAsync(ProductUpdatedEvent productUpdatedEvent, Guid? messageId = null, CancellationToken cancellationToken = default)
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
                // Stok kaydı varsa, sadece StockCount belirtilmişse stok sayısını güncelle
                if (productUpdatedEvent.StockCount.HasValue)
                {
                    stock.Count = productUpdatedEvent.StockCount.Value;
                    await _stockRepository.UpdateAsync(stock, cancellationToken);
                }
                // StockCount belirtilmemişse, stok sayısını değiştirme (sadece ürün bilgileri güncellendi)
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

    public async Task HandleProductDeletedAsync(ProductDeletedEvent productDeletedEvent, Guid? messageId = null, CancellationToken cancellationToken = default)
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
