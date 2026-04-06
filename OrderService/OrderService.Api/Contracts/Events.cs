namespace Contracts.Events;

public record OrderCreatedEvent
{
    public int OrderId { get; init; }
    public int BuyerId { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<OrderItemEvent> Items { get; init; } = new();
}

public record OrderCancelledEvent
{
    public int OrderId { get; init; }
    public int BuyerId { get; init; }
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
    public List<OrderItemEvent> Items { get; init; } = new();
}

public record OrderItemEvent
{
    public int ProductId { get; init; }
    public int SellerId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Subtotal { get; init; }
}
