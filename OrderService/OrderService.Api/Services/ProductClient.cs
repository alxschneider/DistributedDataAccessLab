using System.Net;
using System.Text;
using System.Text.Json;

namespace OrderService.Api.Services;

public class ProductClient : IProductClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}");
        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<ProductInfo?> GetProductAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}");
        if (response.StatusCode != HttpStatusCode.OK) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductInfo>(json, _jsonOptions);
    }

    public async Task<bool> DecrementStockAsync(int productId, int quantity)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { quantity }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PatchAsync($"api/products/{productId}/stock/decrement", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IncrementStockAsync(int productId, int quantity)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { quantity }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PatchAsync($"api/products/{productId}/stock/increment", content);
        return response.IsSuccessStatusCode;
    }
}
