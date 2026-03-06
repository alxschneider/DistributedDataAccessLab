using System.Net.Http.Json;
using WebFrontend.Models;

namespace WebFrontend.Services;

public class CustomerService
{
    private readonly HttpClient _http;
    public CustomerService(HttpClient http) => _http = http;

    public async Task<List<Customer>> GetAllAsync(string? role = null, string? search = null)
    {
        var query = "api/customers?";
        if (!string.IsNullOrEmpty(role)) query += $"role={role}&";
        if (!string.IsNullOrEmpty(search)) query += $"search={search}&";
        return await _http.GetFromJsonAsync<List<Customer>>(query) ?? new();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        var resp = await _http.GetAsync($"api/customers/{id}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<Customer>() : null;
    }

    public async Task<HttpResponseMessage> CreateAsync(Customer customer)
        => await _http.PostAsJsonAsync("api/customers", customer);

    public async Task<HttpResponseMessage> UpdateAsync(int id, Customer customer)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"api/customers/{id}");
        req.Headers.Add("X-User-Id", id.ToString());
        req.Content = JsonContent.Create(customer);
        return await _http.SendAsync(req);
    }

    public async Task<HttpResponseMessage> DeleteAsync(int id)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"api/customers/{id}");
        req.Headers.Add("X-User-Id", id.ToString());
        return await _http.SendAsync(req);
    }

    public async Task<object?> GetDashboardAsync()
        => await _http.GetFromJsonAsync<object>("api/customers/dashboard");

    public async Task<object?> GetPersonalDashboardAsync(int id)
        => await _http.GetFromJsonAsync<object>($"api/customers/{id}/dashboard");
}
