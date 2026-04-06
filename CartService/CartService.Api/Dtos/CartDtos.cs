namespace CartService.Api.Dtos;

// Request DTOs
public record AddCartItemDto(int ProductId, int Quantity = 1);
public record UpdateCartItemDto(int Quantity);

// Response DTOs
public record CartResponseDto(
    int Id,
    int BuyerId,
    List<CartItemResponseDto> Items,
    decimal Total,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CartItemResponseDto(
    int Id,
    int ProductId,
    int SellerId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercent
);

public record EmptyCartDto(
    int BuyerId,
    object[] Items,
    decimal Total
);

public record CheckoutResponseDto(string Message, int? OrderId);
