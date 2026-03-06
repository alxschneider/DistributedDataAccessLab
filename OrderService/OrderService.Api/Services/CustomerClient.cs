using System.Net;
using System.Text.Json;

namespace OrderService.Api.Services;

public class CustomerClient : ICustomerClient
{
    private readonly HttpClient _httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        var response = await _httpClient.GetAsync($"api/customers/{customerId}");
        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> IsBuyerAsync(int customerId)
    {
        var response = await _httpClient.GetAsync($"api/customers/{customerId}");
        if (response.StatusCode != HttpStatusCode.OK) return false;
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var role = doc.RootElement.GetProperty("role").GetString();
        return role == "Buyer";
    }
}
