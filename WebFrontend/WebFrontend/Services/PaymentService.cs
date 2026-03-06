using System.Net.Http.Json;
using WebFrontend.Models;

namespace WebFrontend.Services;

public class PaymentService
{
    private readonly HttpClient _http;
    public PaymentService(HttpClient http) => _http = http;

    public async Task<List<Payment>> GetAllAsync(int? orderId = null, int? buyerId = null, string? status = null)
    {
        var query = "api/payments?";
        if (orderId.HasValue) query += $"orderId={orderId}&";
        if (buyerId.HasValue) query += $"buyerId={buyerId}&";
        if (!string.IsNullOrEmpty(status)) query += $"status={status}&";
        return await _http.GetFromJsonAsync<List<Payment>>(query) ?? new();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        var resp = await _http.GetAsync($"api/payments/{id}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<Payment>() : null;
    }

    public async Task<HttpResponseMessage> CreateAsync(Payment payment)
        => await _http.PostAsJsonAsync("api/payments", new
        {
            payment.OrderId,
            payment.BuyerId,
            payment.Amount,
            payment.Method
        });

    public async Task<HttpResponseMessage> ProcessAsync(int id)
        => await _http.PostAsync($"api/payments/{id}/process", null);

    public async Task<HttpResponseMessage> RefundAsync(int id)
        => await _http.PostAsync($"api/payments/{id}/refund", null);
}
