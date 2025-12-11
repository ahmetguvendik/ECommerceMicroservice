using MediatR;
using BasketService.Application.Features.Commands.BasketCommands;
using BasketService.Application.Repositories;
using BasketService.Application.Services;
using BasketService.Application.UnitOfWorks;
using BasketService.Domain.Entities;

namespace BasketService.Application.Features.Handlers.BasketHandlers.Write;

public class AddItemToBasketCommandHandler : IRequestHandler<AddItemToBasketCommand>
{
    private readonly IGenericRepository<Basket> _basketRepository;
    private readonly IGenericRepository<BasketItem> _basketItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IStockService _stockService;

    public AddItemToBasketCommandHandler(
        IGenericRepository<Basket> basketRepository,
        IGenericRepository<BasketItem> basketItemRepository,
        IUnitOfWork unitOfWork,
        IProductService productService,
        IStockService stockService)
    {
        _basketRepository = basketRepository;
        _basketItemRepository = basketItemRepository;
        _unitOfWork = unitOfWork;
        _productService = productService;
        _stockService = stockService;
    }
    
    public async Task Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            // 1. CustomerId ile sepeti bul veya oluştur
            var baskets = await _basketRepository.GetAllAsync(
                b => b.CustomerId == request.CustomerId, 
                cancellationToken);
            
            var basket = baskets.FirstOrDefault();
            
            if (basket == null)
            {
                // Sepet yoksa oluştur
                basket = new Basket
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId
                };
                await _basketRepository.CreateAsync(basket, cancellationToken);
            }

            // 2. Ürün bilgilerini ProductService'ten al
            var product = await _productService.GetProductByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                throw new ArgumentException($"Product with Id {request.ProductId} not found.");
            }

            if (!product.IsActive)
            {
                throw new ArgumentException($"Product with Id {request.ProductId} is not active.");
            }

            // 3. Sepette bu ürün var mı kontrol et
            var existingItems = await _basketItemRepository.GetAllAsync(
                item => item.BasketId == basket.Id && item.ProductId == request.ProductId,
                cancellationToken);
            
            var existingItem = existingItems.FirstOrDefault();
            
            if (existingItem != null)
            {
                // Aynı ürün varsa, sadece miktarı artır
                var newQuantity = existingItem.Quantity + request.Quantity;
                
                // Stok kontrolü yap (yeni toplam miktar için)
                var isStockAvailable = await _stockService.CheckStockAvailabilityAsync(request.ProductId, newQuantity, cancellationToken);
                if (!isStockAvailable)
                {
                    throw new ArgumentException($"Insufficient stock for ProductId {request.ProductId}. Requested total quantity: {newQuantity}");
                }
                
                existingItem.Quantity = newQuantity;
                await _basketItemRepository.UpdateAsync(existingItem, cancellationToken);
            }
            else
            {
                // Yeni ürün ise, stok kontrolü yap
                var isStockAvailable = await _stockService.CheckStockAvailabilityAsync(request.ProductId, request.Quantity, cancellationToken);
                if (!isStockAvailable)
                {
                    throw new ArgumentException($"Insufficient stock for ProductId {request.ProductId}. Requested quantity: {request.Quantity}");
                }
                
                // 4. BasketItem oluştur
                var basketItem = new BasketItem
                {
                    Id = Guid.NewGuid(),
                    BasketId = basket.Id,
                    ProductId = request.ProductId,
                    ProductName = product.Name,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price
                };
                
                await _basketItemRepository.CreateAsync(basketItem, cancellationToken);
                
                // 5. Basket'e item ekle
                basket.Items.Add(basketItem);
            }
            
            // 6. Basket'i güncelle
            await _basketRepository.UpdateAsync(basket, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

