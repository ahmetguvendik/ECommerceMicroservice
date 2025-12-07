namespace ProductService.Domain.Entities;

public class Product : BaseEntity
{ 
    public string Name { get; set; }
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Sku { get; set; }       
    public string Barcode { get; set; }  

    public bool IsActive { get; set; }
    public ProductCategory ProductCategory { get; set; }
    public Guid ProductCategoryId { get; set; }
}