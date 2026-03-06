namespace OrderService.Api.Models;

public class CreateOrderRequest
{
    public int BuyerId { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

public class ReturnOrderRequest
{
    public string? Reason { get; set; }
}
