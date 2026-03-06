namespace NotificationService.Api.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; } // Recipient
    public string Type { get; set; } = string.Empty; // OrderCreated, PaymentApproved, OrderCancelled, etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
