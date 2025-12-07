namespace BasketService.Domain.Entities;

public class BasketItem : BaseEntity
{
    public Guid BasketId { get; set; }
    public Basket Basket { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

