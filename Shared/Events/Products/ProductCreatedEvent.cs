namespace Shared.Events;

public class ProductCreatedEvent 
{
    public Guid ProdcutId { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public int InitialStockCount { get; set; }
    public Guid IdempotentToken { get; set; }   
}