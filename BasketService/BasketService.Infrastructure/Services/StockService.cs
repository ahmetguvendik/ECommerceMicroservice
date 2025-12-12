using System.Text.Json;
using BasketService.Application.DTOs;
using BasketService.Application.Services;
using Microsoft.Extensions.Configuration;

namespace BasketService.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public StockService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        var stockServiceUrl = _configuration["ServiceUrls:StockService"];
        _httpClient.BaseAddress = new Uri(stockServiceUrl);
    }

    public async Task<StockDto?> GetStockByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Stock/product/{productId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<StockDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CheckStockAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var stock = await GetStockByProductIdAsync(productId, cancellationToken);
        
        if (stock == null)
            return false;

        return stock.Count >= quantity;
    }
}
