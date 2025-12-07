namespace BasketService.Domain.Entities;

public class Basket : BaseEntity
{
    public Guid CustomerId { get; set; }
    public List<BasketItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
}

