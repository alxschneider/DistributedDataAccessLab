using System.Text;
using System.Text.Json;

namespace OrderService.Api.Services;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;

    public NotificationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendNotificationAsync(int userId, string type, string title, string message)
    {
        var payload = new { userId, type, title, message };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        try
        {
            await _httpClient.PostAsync("api/notifications", content);
        }
        catch
        {
            // Fire and forget — notification failures should not block orders
        }
    }
}
