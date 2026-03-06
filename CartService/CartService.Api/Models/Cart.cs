namespace CartService.Api.Models;

public class Cart
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<CartItem> Items { get; set; } = new();
}
