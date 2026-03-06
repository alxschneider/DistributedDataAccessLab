using System.Net;
using System.Text.Json;

namespace CartService.Api.Services;

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

    public async Task<ProductInfo?> GetProductAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}");
        if (response.StatusCode != HttpStatusCode.OK) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductInfo>(json, _jsonOptions);
    }
}
