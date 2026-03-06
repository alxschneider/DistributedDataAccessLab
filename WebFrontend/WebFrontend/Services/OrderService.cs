using System.Net.Http.Json;
using WebFrontend.Models;

namespace WebFrontend.Services;

public class OrderService
{
    private readonly HttpClient _http;
    public OrderService(HttpClient http) => _http = http;

    public async Task<List<Order>> GetAllAsync(int? buyerId = null, int? sellerId = null, string? status = null)
    {
        var query = "api/orders?";
        if (buyerId.HasValue) query += $"buyerId={buyerId}&";
        if (sellerId.HasValue) query += $"sellerId={sellerId}&";
        if (!string.IsNullOrEmpty(status)) query += $"status={status}&";
        return await _http.GetFromJsonAsync<List<Order>>(query) ?? new();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        var resp = await _http.GetAsync($"api/orders/{id}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<Order>() : null;
    }

    public async Task<HttpResponseMessage> CreateAsync(CreateOrderRequest order)
        => await _http.PostAsJsonAsync("api/orders", order);

    public async Task<HttpResponseMessage> UpdateStatusAsync(int id, string status)
        => await _http.PutAsJsonAsync($"api/orders/{id}/status", new { status });

    public async Task<HttpResponseMessage> CancelAsync(int id, string? reason = null)
        => await _http.PostAsJsonAsync($"api/orders/{id}/cancel", new { reason });

    public async Task<HttpResponseMessage> ReturnAsync(int id, string? reason = null)
        => await _http.PostAsJsonAsync($"api/orders/{id}/return", new { reason });
}
