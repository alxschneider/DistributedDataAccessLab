using System.Text.Json;

namespace CustomerService.Api.Services;

public class ProductStatsClient : IProductStatsClient
{
    private readonly HttpClient _httpClient;

    public ProductStatsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<object?> GetSellerProductsDashboardAsync(int sellerId)
    {
        var response = await _httpClient.GetAsync($"api/products/seller/{sellerId}/dashboard");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<object>(json);
    }
}
