namespace Shared.Events;

public class ProductUpdatedEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; }       
    public bool IsActive { get; set; }
    public Guid ProductCategoryId { get; set; }
}