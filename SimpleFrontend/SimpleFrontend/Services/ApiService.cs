using System.Net.Http.Json;
using System.Text.Json;
using SimpleFrontend.Models;

namespace SimpleFrontend.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    // ----- Customers -----
    public async Task<List<CustomerDto>> GetCustomersAsync()
        => await _http.GetFromJsonAsync<List<CustomerDto>>("/api/customers") ?? new();

    // ----- Products -----
    public async Task<List<ProductDto>> GetProductsAsync()
        => await _http.GetFromJsonAsync<List<ProductDto>>("/api/products") ?? new();

    // ----- Orders -----
    public async Task<List<OrderDto>> GetOrdersAsync()
        => await _http.GetFromJsonAsync<List<OrderDto>>("/api/orders") ?? new();

    public async Task<HttpResponseMessage> CreateOrderAsync(object order)
        => await _http.PostAsJsonAsync("/api/orders", order);

    // ----- Cart -----
    public async Task<CartDto?> GetCartAsync(int buyerId)
        => await _http.GetFromJsonAsync<CartDto>($"/api/cart/{buyerId}");

    public async Task<HttpResponseMessage> AddCartItemAsync(int buyerId, object item)
        => await _http.PostAsJsonAsync($"/api/cart/{buyerId}/items", item);

    // ----- Payments -----
    public async Task<List<PaymentDto>> GetPaymentsAsync()
        => await _http.GetFromJsonAsync<List<PaymentDto>>("/api/payments") ?? new();

    // ----- Notifications -----
    public async Task<NotificationPageDto?> GetNotificationsAsync(int userId)
        => await _http.GetFromJsonAsync<NotificationPageDto>($"/api/notifications/{userId}");

    // ----- Health (generic) -----
    public async Task<(bool ok, int count)> CheckEndpointAsync(string endpoint)
    {
        try
        {
            var resp = await _http.GetAsync(endpoint);
            if (!resp.IsSuccessStatusCode)
                return (false, -1);

            if (endpoint.Contains("health"))
                return (true, -1);

            var el = await resp.Content.ReadFromJsonAsync<JsonElement>();
            var count = el.ValueKind == JsonValueKind.Array ? el.GetArrayLength() : -1;
            return (true, (int)count);
        }
        catch
        {
            return (false, -1);
        }
    }
}
