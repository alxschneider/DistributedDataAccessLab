using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/aggregate")]
public class AggregateController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AggregateController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Aggregated endpoint: returns order details combined with customer and product info.
    /// Calls OrderService, CustomerService, and ProductService in parallel.
    /// </summary>
    [HttpGet("order-details/{orderId}")]
    public async Task<IActionResult> GetOrderDetails(int orderId)
    {
        var orderClient = _httpClientFactory.CreateClient("OrderService");
        var customerClient = _httpClientFactory.CreateClient("CustomerService");
        var productClient = _httpClientFactory.CreateClient("ProductService");

        // Fetch order
        var orderResponse = await orderClient.GetAsync($"api/orders/{orderId}");
        if (!orderResponse.IsSuccessStatusCode)
            return NotFound(new { Message = $"Order #{orderId} not found." });

        var orderJson = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderJson, _jsonOptions);

        // Extract buyerId and product IDs from the order
        var buyerId = order.GetProperty("buyerId").GetInt32();
        var items = order.GetProperty("items").EnumerateArray().ToList();

        // Fetch customer and products in parallel
        var customerTask = customerClient.GetAsync($"api/customers/{buyerId}");
        var productIds = items
            .Select(i => i.GetProperty("productId").GetInt32())
            .Distinct()
            .ToList();

        var productTasks = productIds.Select(async pid =>
        {
            var resp = await productClient.GetAsync($"api/products/{pid}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                return (ProductId: pid, Data: (JsonElement?)JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions));
            }
            return (ProductId: pid, Data: (JsonElement?)null);
        }).ToList();

        var customerResponse = await customerTask;
        var productResults = await Task.WhenAll(productTasks);

        // Build customer info
        object? customerInfo = null;
        if (customerResponse.IsSuccessStatusCode)
        {
            var customerJson = await customerResponse.Content.ReadAsStringAsync();
            customerInfo = JsonSerializer.Deserialize<JsonElement>(customerJson, _jsonOptions);
        }

        // Build products map
        var productsMap = productResults
            .Where(p => p.Data.HasValue)
            .ToDictionary(p => p.ProductId, p => (object)p.Data!.Value);

        return Ok(new
        {
            Order = order,
            Customer = customerInfo,
            Products = productsMap
        });
    }
}
