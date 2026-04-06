namespace ProductService.Api.Dtos;

// Request DTOs
public record CreateProductDto(
    int SellerId,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int Stock,
    decimal DiscountPercent,
    string ImageUrl
);

public record UpdateProductDto(
    string Name,
    string Description,
    string Category,
    decimal Price,
    string ImageUrl
);

public record StockUpdateDto(int Quantity);
public record StockDecrementDto(int Quantity);
public record DiscountUpdateDto(decimal DiscountPercent);

// Response DTOs
public record ProductResponseDto(
    int Id,
    int SellerId,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int Stock,
    decimal DiscountPercent,
    decimal FinalPrice,
    string ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProductStockDto(int Id, int Stock);
public record ProductDiscountDto(int Id, decimal DiscountPercent, decimal FinalPrice);

public record ProductDashboardDto(
    int TotalProducts,
    decimal AveragePrice,
    decimal AverageStock,
    int OutOfStock,
    int WithDiscount,
    IEnumerable<CategoryStatsDto> ByCategory
);

public record CategoryStatsDto(string Category, int Count, decimal AvgPrice);

public record SellerDashboardDto(
    int TotalProducts,
    decimal AveragePrice,
    int TotalStock,
    int OutOfStock,
    int WithDiscount,
    decimal TotalValueInStock
);

public record SellerProductDto(
    int Id,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    decimal DiscountPercent,
    decimal FinalPrice,
    DateTime CreatedAt
);
