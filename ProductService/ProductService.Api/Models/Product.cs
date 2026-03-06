using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Api.Models;

public class Product
{
    public int Id { get; set; }
    public int SellerId { get; set; } // Logical FK → CustomerService
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal DiscountPercent { get; set; } // 0-100
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public decimal FinalPrice => Math.Round(Price * (1 - DiscountPercent / 100), 2);
}
