using System.Net.Http.Json;
using WebFrontend.Models;

namespace WebFrontend.Services;

public class ProductService
{
    private readonly HttpClient _http;
    public ProductService(HttpClient http) => _http = http;

    public async Task<List<Product>> GetAllAsync(string? category = null, string? search = null,
        decimal? minPrice = null, decimal? maxPrice = null, int? sellerId = null)
    {
        var query = "api/products?";
        if (!string.IsNullOrEmpty(category)) query += $"category={category}&";
        if (!string.IsNullOrEmpty(search)) query += $"search={search}&";
        if (minPrice.HasValue) query += $"minPrice={minPrice}&";
        if (maxPrice.HasValue) query += $"maxPrice={maxPrice}&";
        if (sellerId.HasValue) query += $"sellerId={sellerId}&";
        return await _http.GetFromJsonAsync<List<Product>>(query) ?? new();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var resp = await _http.GetAsync($"api/products/{id}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<Product>() : null;
    }

    public async Task<HttpResponseMessage> CreateAsync(Product product)
        => await _http.PostAsJsonAsync("api/products", product);

    public async Task<HttpResponseMessage> UpdateAsync(int id, Product product, int userId)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"api/products/{id}");
        req.Headers.Add("X-User-Id", userId.ToString());
        req.Content = JsonContent.Create(product);
        return await _http.SendAsync(req);
    }

    public async Task<HttpResponseMessage> DeleteAsync(int id, int userId)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"api/products/{id}");
        req.Headers.Add("X-User-Id", userId.ToString());
        return await _http.SendAsync(req);
    }

    public async Task<HttpResponseMessage> UpdateStockAsync(int id, int quantity)
        => await _http.PatchAsJsonAsync($"api/products/{id}/stock", new { quantity });

    public async Task<HttpResponseMessage> UpdateDiscountAsync(int id, decimal discountPercent)
        => await _http.PatchAsJsonAsync($"api/products/{id}/discount", new { discountPercent });
}
