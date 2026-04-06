using Contracts.Events;
using MassTransit;
using NotificationService.Api.Data;
using NotificationService.Api.Models;

namespace NotificationService.Api.Consumers;

public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(NotificationDbContext context, ILogger<OrderCancelledConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("[RabbitMQ] OrderCancelledEvent received for Order #{OrderId}, Buyer #{BuyerId}", evt.OrderId, evt.BuyerId);

        // Create notification for buyer
        _context.Notifications.Add(new Notification
        {
            UserId = evt.BuyerId,
            Type = "OrderCancelled",
            Title = "Order cancelled (async)",
            Message = $"Your order #{evt.OrderId} has been cancelled via async messaging. Reason: {evt.Reason ?? "N/A"}",
            CreatedAt = DateTime.UtcNow
        });

        // Notify sellers
        var sellerIds = evt.Items.Select(i => i.SellerId).Distinct();
        foreach (var sellerId in sellerIds)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = sellerId,
                Type = "OrderCancelled",
                Title = "Order cancelled (async)",
                Message = $"Order #{evt.OrderId} was cancelled. Stock has been restored.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
