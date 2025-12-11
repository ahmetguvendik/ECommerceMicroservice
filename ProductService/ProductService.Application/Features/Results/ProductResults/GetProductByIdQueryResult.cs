namespace ProductService.Application.Features.Results;

public class GetProductByIdQueryResult
{
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
        public string Barcode { get; set; }
        public bool IsActive { get; set; }
        public Guid ProductCategoryId { get; set; }
        
}