using System.Net.Http.Json;
using WebFrontend.Models;

namespace WebFrontend.Services;

public class CartService
{
    private readonly HttpClient _http;
    public CartService(HttpClient http) => _http = http;

    public async Task<CartResponse?> GetCartAsync(int buyerId)
    {
        var resp = await _http.GetAsync($"api/cart/{buyerId}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<CartResponse>() : null;
    }

    public async Task<HttpResponseMessage> AddItemAsync(int buyerId, int productId, int quantity)
        => await _http.PostAsJsonAsync($"api/cart/{buyerId}/items", new { productId, quantity });

    public async Task<HttpResponseMessage> UpdateItemAsync(int buyerId, int itemId, int quantity)
        => await _http.PutAsJsonAsync($"api/cart/{buyerId}/items/{itemId}", new { quantity });

    public async Task<HttpResponseMessage> RemoveItemAsync(int buyerId, int itemId)
        => await _http.DeleteAsync($"api/cart/{buyerId}/items/{itemId}");

    public async Task<HttpResponseMessage> ClearCartAsync(int buyerId)
        => await _http.DeleteAsync($"api/cart/{buyerId}");

    public async Task<HttpResponseMessage> CheckoutAsync(int buyerId)
        => await _http.PostAsync($"api/cart/{buyerId}/checkout", null);
}

public class CartResponse
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
