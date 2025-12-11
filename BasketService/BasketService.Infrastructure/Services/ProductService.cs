using System.Text.Json;
using BasketService.Application.DTOs;
using BasketService.Application.Services;
using Microsoft.Extensions.Configuration;

namespace BasketService.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ProductService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        var productServiceUrl = _configuration["ServiceUrls:ProductService"] ?? "https://localhost:7001";
        _httpClient.BaseAddress = new Uri(productServiceUrl);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Product/{productId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}
