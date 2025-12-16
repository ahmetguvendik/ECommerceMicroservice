using System;
using System.Linq;
using Shared.Messages;
using StockService.Application.Repositories;
using StockService.Application.Services;
using StockService.Application.UnitOfWorks;
using StockService.Domain.Entities;

namespace StockService.Infrastructure.Services;

public class StockEventService : IStockEventService
{
    private readonly IGenericRepository<Stock> _stockRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockEventService(IGenericRepository<Stock> stockRepository, IUnitOfWork unitOfWork)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task HandleStockRollbackMessage(StockRollbackMessage message, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var items = message.OrderItemMessages ?? [];
            var productIds = items.Select(x => x.ProductId).ToList();

            var stocks = await _stockRepository.GetAllAsync(s => productIds.Contains(s.ProductId), cancellationToken);

            foreach (var item in items)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);
                if (stock == null)
                {
                    throw new InvalidOperationException($"Stock record not found for product {item.ProductId} during rollback.");
                }

                stock.Count += item.Quantity;
                await _stockRepository.UpdateAsync(stock, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}