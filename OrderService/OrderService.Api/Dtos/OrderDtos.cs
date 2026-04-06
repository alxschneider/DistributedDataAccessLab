namespace OrderService.Api.Dtos;

// Request DTOs (moved from Models/Dtos.cs)
public record CreateOrderRequestDto(
    int BuyerId,
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    int ProductId,
    int Quantity
);

public record UpdateStatusDto(string Status);
public record CancelOrderDto(string? Reason);
public record ReturnOrderDto(string? Reason);

// Response DTOs
public record OrderResponseDto(
    int Id,
    int BuyerId,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CancelledAt,
    DateTime? ReturnedAt,
    string? CancellationReason,
    string? ReturnReason,
    List<OrderItemResponseDto> Items
);

public record OrderItemResponseDto(
    int Id,
    int ProductId,
    int SellerId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Subtotal
);

public record OrderDashboardDto(
    int TotalOrders,
    IEnumerable<StatusCountDto> ByStatus,
    decimal AverageTicket,
    decimal TotalRevenue,
    int CancelledLast7Days,
    int CancelledLast30Days,
    int ReturnedLast30Days,
    double CancellationRate,
    double ReturnRate
);

public record StatusCountDto(string Status, int Count);

public record BuyerDashboardDto(
    int TotalOrders,
    decimal TotalSpent,
    IEnumerable<StatusCountDto> ByStatus,
    LastOrderDto? LastOrder
);

public record LastOrderDto(int Id, string Status, decimal Total, DateTime CreatedAt);

public record SellerDashboardDto(
    int TotalOrders,
    int TotalItemsSold,
    decimal TotalRevenue,
    IEnumerable<StatusCountDto> ByStatus,
    decimal AverageOrderValue
);
