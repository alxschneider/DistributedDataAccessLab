namespace PaymentService.Api.Dtos;

// Request DTOs
public record CreatePaymentDto(
    int OrderId,
    int BuyerId,
    decimal Amount,
    string Method = "CreditCard"
);

// Response DTOs
public record PaymentResponseDto(
    int Id,
    int OrderId,
    int BuyerId,
    decimal Amount,
    string Method,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record PaymentProcessResultDto(
    int Id,
    int OrderId,
    string Status,
    string Method,
    decimal Amount,
    DateTime? ProcessedAt,
    int SimulatedDelayMs
);

public record PaymentDashboardDto(
    int TotalPayments,
    decimal TotalRevenue,
    decimal TotalRefunded,
    IEnumerable<PaymentStatusStatsDto> ByStatus,
    IEnumerable<PaymentMethodStatsDto> ByMethod,
    double ApprovalRate
);

public record PaymentStatusStatsDto(string Status, int Count, decimal Total);
public record PaymentMethodStatsDto(string Method, int Count, decimal Total);
