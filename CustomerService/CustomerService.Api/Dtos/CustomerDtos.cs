namespace CustomerService.Api.Dtos;

// Request DTOs
public record CreateCustomerDto(
    string Name,
    string Email,
    string Phone,
    string Document,
    string Role,
    string Address
);

public record UpdateCustomerDto(
    string Name,
    string Email,
    string Phone,
    string Document,
    string Address
);

// Response DTOs
public record CustomerResponseDto(
    int Id,
    string Name,
    string Email,
    string Phone,
    string Role,
    string Address,
    DateTime CreatedAt
);

public record CustomerDetailDto(
    int Id,
    string Name,
    string Email,
    string Phone,
    string Document,
    string Role,
    string Address,
    bool IsActive,
    DateTime CreatedAt
);

public record CustomerDashboardDto(
    int TotalBuyers,
    int TotalSellers,
    int NewBuyersLast30Days,
    int NewSellersLast30Days,
    int TotalInactive
);

public record PersonalDashboardDto(
    CustomerSummaryDto Customer,
    object? OrderStats,
    object? ProductStats
);

public record CustomerSummaryDto(
    int Id,
    string Name,
    string Role
);
