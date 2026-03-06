using System.Text;
using System.Text.Json;

namespace PaymentService.Api.Services;

public class OrderClient : IOrderClient
{
    private readonly HttpClient _httpClient;

    public OrderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { status }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PutAsync($"api/orders/{orderId}/status", content);
        return response.IsSuccessStatusCode;
    }
}
