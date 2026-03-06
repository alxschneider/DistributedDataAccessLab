using System.Text.Json;

namespace CustomerService.Api.Services;

public class OrderStatsClient : IOrderStatsClient
{
    private readonly HttpClient _httpClient;

    public OrderStatsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<object?> GetBuyerDashboardAsync(int buyerId)
    {
        var response = await _httpClient.GetAsync($"api/orders/buyer/{buyerId}/dashboard");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<object>(json);
    }

    public async Task<object?> GetSellerDashboardAsync(int sellerId)
    {
        var response = await _httpClient.GetAsync($"api/orders/seller/{sellerId}/dashboard");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<object>(json);
    }
}
