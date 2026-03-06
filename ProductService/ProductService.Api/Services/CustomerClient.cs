using System.Net;
using System.Text.Json;

namespace ProductService.Api.Services;

public class CustomerClient : ICustomerClient
{
    private readonly HttpClient _httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SellerExistsAsync(int sellerId)
    {
        var response = await _httpClient.GetAsync($"api/customers/{sellerId}");
        if (response.StatusCode != HttpStatusCode.OK) return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var role = doc.RootElement.GetProperty("role").GetString();
        return role == "Seller";
    }
}
