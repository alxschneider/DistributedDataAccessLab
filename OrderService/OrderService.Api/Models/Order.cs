namespace OrderService.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int BuyerId { get; set; } // Logical FK → CustomerService
    public string Status { get; set; } = "AwaitingPayment";
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? ReturnReason { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; } // Logical FK → ProductService
    public int SellerId { get; set; } // Logical FK → CustomerService
    public string ProductName { get; set; } = string.Empty; // Snapshot
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Snapshot of price at purchase
    public decimal Discount { get; set; } // Discount amount per unit
    public decimal Subtotal { get; set; } // (UnitPrice - Discount) * Quantity

    public Order Order { get; set; } = null!;
}
