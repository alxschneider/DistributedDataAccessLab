namespace SimpleFrontend.Models;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Role { get; set; } = "";
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public string Status { get; set; } = "";
    public decimal Total { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto>? Items { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class CartDto
{
    public int BuyerId { get; set; }
    public decimal Total { get; set; }
    public List<CartItemDto>? Items { get; set; }
}

public class CartItemDto
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SellerId { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class NotificationPageDto
{
    public int Total { get; set; }
    public List<NotificationDto>? Items { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
}
