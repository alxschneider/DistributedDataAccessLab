using System.Net.Http.Json;
using WebFrontend.Models;

namespace WebFrontend.Services;

public class NotificationService
{
    private readonly HttpClient _http;
    public NotificationService(HttpClient http) => _http = http;

    public async Task<List<Notification>> GetByUserAsync(int userId, int page = 1, int pageSize = 20)
    {
        var result = await _http.GetFromJsonAsync<PaginatedNotifications>(
            $"api/notifications/{userId}?page={page}&pageSize={pageSize}");
        return result?.Items ?? new();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        var resp = await _http.GetAsync($"api/notifications/{userId}/unread-count");
        if (!resp.IsSuccessStatusCode) return 0;
        var result = await resp.Content.ReadFromJsonAsync<UnreadCountResponse>();
        return result?.UnreadCount ?? 0;
    }

    public async Task<HttpResponseMessage> MarkAsReadAsync(int id)
        => await _http.PutAsync($"api/notifications/{id}/read", null);

    public async Task<HttpResponseMessage> MarkAllAsReadAsync(int userId)
        => await _http.PutAsync($"api/notifications/{userId}/read-all", null);

    public async Task<HttpResponseMessage> DeleteAsync(int id)
        => await _http.DeleteAsync($"api/notifications/{id}");
}

public class PaginatedNotifications
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<Notification> Items { get; set; } = new();
}

public class UnreadCountResponse
{
    public int UnreadCount { get; set; }
}
