using System.Text;
using System.Text.Json;

namespace OrderService.Api.Services;

public class PaymentClient : IPaymentClient
{
    private readonly HttpClient _httpClient;

    public PaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> RequestRefundAsync(int orderId)
    {
        try
        {
            // Find payment by orderId and refund it
            var response = await _httpClient.GetAsync($"api/payments?orderId={orderId}");
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var payments = doc.RootElement.EnumerateArray();

            foreach (var payment in payments)
            {
                var paymentId = payment.GetProperty("id").GetInt32();
                var status = payment.GetProperty("status").GetString();
                if (status == "Approved")
                {
                    await _httpClient.PostAsync($"api/payments/{paymentId}/refund", null);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
