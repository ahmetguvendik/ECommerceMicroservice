namespace Shared.Events;

public class ProductCreatedEvent 
{
    public Guid ProdcutId { get; set; }
    public Guid ProductCategoryId { get; set; }
}