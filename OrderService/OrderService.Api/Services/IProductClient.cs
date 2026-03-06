namespace OrderService.Api.Services;

public class ProductInfo
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal FinalPrice { get; set; }
    public int Stock { get; set; }
    public decimal DiscountPercent { get; set; }
}

public interface IProductClient
{
    Task<bool> ProductExistsAsync(int productId);
    Task<ProductInfo?> GetProductAsync(int productId);
    Task<bool> DecrementStockAsync(int productId, int quantity);
    Task<bool> IncrementStockAsync(int productId, int quantity);
}
