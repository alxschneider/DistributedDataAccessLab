namespace PaymentService.Api.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "CreditCard"; // CreditCard, Pix, Boleto
    public string Status { get; set; } = "Pending"; // Pending, Processing, Approved, Rejected, Refunded
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
