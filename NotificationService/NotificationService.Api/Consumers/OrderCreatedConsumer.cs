using Contracts.Events;
using MassTransit;
using NotificationService.Api.Data;
using NotificationService.Api.Models;

namespace NotificationService.Api.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(NotificationDbContext context, ILogger<OrderCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("[RabbitMQ] OrderCreatedEvent received for Order #{OrderId}, Buyer #{BuyerId}", evt.OrderId, evt.BuyerId);

        // Create notification for buyer
        _context.Notifications.Add(new Notification
        {
            UserId = evt.BuyerId,
            Type = "OrderCreated",
            Title = "Order confirmed (async)",
            Message = $"Your order #{evt.OrderId} (Total: $ {evt.Total:F2}) has been confirmed via async messaging.",
            CreatedAt = DateTime.UtcNow
        });

        // Create notifications for each seller
        var sellerIds = evt.Items.Select(i => i.SellerId).Distinct();
        foreach (var sellerId in sellerIds)
        {
            var sellerItems = evt.Items.Where(i => i.SellerId == sellerId).ToList();
            var itemNames = string.Join(", ", sellerItems.Select(i => i.ProductName));
            _context.Notifications.Add(new Notification
            {
                UserId = sellerId,
                Type = "OrderCreated",
                Title = "New order received (async)",
                Message = $"Order #{evt.OrderId} includes: {itemNames}. Total: $ {sellerItems.Sum(i => i.Subtotal):F2}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
