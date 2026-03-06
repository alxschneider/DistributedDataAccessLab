namespace CartService.Api.Services;

public interface IOrderClient
{
    Task<int?> CreateOrderAsync(int buyerId, List<OrderItemRequest> items);
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
