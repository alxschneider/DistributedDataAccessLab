using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;
    private readonly INotificationClient _notificationClient;
    private readonly IPaymentClient _paymentClient;

    // Valid status transitions
    private static readonly Dictionary<string, string[]> _validTransitions = new()
    {
        ["AwaitingPayment"] = ["Paid", "Cancelled"],
        ["Paid"] = ["Shipped", "Cancelled"],
        ["Shipped"] = ["Delivered"],
        ["Delivered"] = ["Returned"],
    };

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient,
        INotificationClient notificationClient,
        IPaymentClient paymentClient)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;
        _notificationClient = notificationClient;
        _paymentClient = paymentClient;
    }

    //!!!IMporant: In a bigger or prod enviroment, its normal to have the business logic in a separate service layer, and the controller just calls that service.
    //  For simplicity, we put all logic in the controller here.!!!

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? buyerId,
        [FromQuery] int? sellerId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var query = _context.Orders.Include(o => o.Items).AsQueryable();

        if (buyerId.HasValue)
            query = query.Where(o => o.BuyerId == buyerId.Value);

        if (sellerId.HasValue)
            query = query.Where(o => o.Items.Any(i => i.SellerId == sellerId.Value));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.CreatedAt <= toDate.Value);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    /// <summary>
    /// Create order from a list of items. Validates buyer, products, stock. Decrements stock.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("Order must have at least one item.");

        // Validate buyer exists
        var isBuyer = await _customerClient.IsBuyerAsync(request.BuyerId);
        if (!isBuyer)
            return BadRequest("Buyer does not exist or is not a Buyer.");

        var order = new Order
        {
            BuyerId = request.BuyerId,
            Status = "AwaitingPayment",
            CreatedAt = DateTime.UtcNow
        };

        // Validate each product and build order items
        foreach (var item in request.Items)
        {
            var product = await _productClient.GetProductAsync(item.ProductId);
            if (product == null)
                return BadRequest($"Product {item.ProductId} does not exist.");

            if (product.Stock < item.Quantity)
                return BadRequest($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {item.Quantity}");

            var discountPerUnit = product.Price - product.FinalPrice;
            var subtotal = product.FinalPrice * item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                SellerId = product.SellerId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Discount = discountPerUnit,
                Subtotal = subtotal
            });
        }

        order.Total = order.Items.Sum(i => i.Subtotal);

        // Decrement stock for each product
        foreach (var item in order.Items)
        {
            var decremented = await _productClient.DecrementStockAsync(item.ProductId, item.Quantity);
            if (!decremented)
                return BadRequest($"Failed to reserve stock for product {item.ProductId}.");
        }

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Send notification to buyer
        await _notificationClient.SendNotificationAsync(
            request.BuyerId,
            "OrderCreated",
            "Order created",
            $"Your order #{order.Id} has been placed successfully. Total: $ {order.Total:F2}");

        // Notify each seller involved
        var sellerIds = order.Items.Select(i => i.SellerId).Distinct();
        foreach (var sellerId in sellerIds)
        {
            var sellerItems = order.Items.Where(i => i.SellerId == sellerId).ToList();
            var itemNames = string.Join(", ", sellerItems.Select(i => i.ProductName));
            await _notificationClient.SendNotificationAsync(
                sellerId,
                "OrderCreated",
                "New order received",
                $"Order #{order.Id} includes your course(s): {itemNames}. Total: $ {sellerItems.Sum(i => i.Subtotal):F2}");
        }

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Update order status with valid transition checks
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
            return NotFound();

        if (!_validTransitions.ContainsKey(order.Status))
            return BadRequest($"Cannot transition from status '{order.Status}'.");

        if (!_validTransitions[order.Status].Contains(request.Status))
            return BadRequest($"Invalid transition: '{order.Status}' → '{request.Status}'. Allowed: {string.Join(", ", _validTransitions[order.Status])}");

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Notify buyer
        await _notificationClient.SendNotificationAsync(
            order.BuyerId,
            $"Order{request.Status}",
            $"Order {request.Status}",
            $"Your order #{order.Id} status is now: {request.Status}");

        // Notify sellers
        var sellerIds = order.Items.Select(i => i.SellerId).Distinct();
        foreach (var sellerId in sellerIds)
        {
            await _notificationClient.SendNotificationAsync(
                sellerId,
                $"Order{request.Status}",
                $"Order #{order.Id} — {request.Status}",
                $"Order #{order.Id} status updated to: {request.Status}");
        }

        return Ok(order);
    }

    /// <summary>
    /// Cancel order. Allowed within 7 days if status is AwaitingPayment or Paid.
    /// Restores stock and requests refund if paid.
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelOrderRequest? request)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
            return NotFound();

        if (order.Status != "AwaitingPayment" && order.Status != "Paid")
            return BadRequest($"Cannot cancel order with status '{order.Status}'. Only 'AwaitingPayment' or 'Paid' orders can be cancelled.");

        var daysSinceCreation = (DateTime.UtcNow - order.CreatedAt).TotalDays;
        if (daysSinceCreation > 7)
            return BadRequest("Cannot cancel order after 7 days.");

        // Restore stock
        foreach (var item in order.Items)
        {
            await _productClient.IncrementStockAsync(item.ProductId, item.Quantity);
        }

        // Request refund if was paid
        if (order.Status == "Paid")
        {
            await _paymentClient.RequestRefundAsync(order.Id);
        }

        order.Status = "Cancelled";
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = request?.Reason;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationClient.SendNotificationAsync(
            order.BuyerId,
            "OrderCancelled",
            "Order cancelled",
            $"Your order #{order.Id} has been cancelled.");

        // Notify sellers
        var sellerIds = order.Items.Select(i => i.SellerId).Distinct();
        foreach (var sellerId in sellerIds)
        {
            await _notificationClient.SendNotificationAsync(
                sellerId,
                "OrderCancelled",
                $"Order #{order.Id} cancelled",
                $"Order #{order.Id} has been cancelled by the buyer.");
        }

        return Ok(order);
    }

    /// <summary>
    /// Return order. Allowed within 30 days after Delivered status.
    /// Restores stock and requests refund.
    /// </summary>
    [HttpPost("{id}/return")]
    public async Task<IActionResult> Return(int id, [FromBody] ReturnOrderRequest? request)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
            return NotFound();

        if (order.Status != "Delivered")
            return BadRequest($"Cannot return order with status '{order.Status}'. Only 'Delivered' orders can be returned.");

        var daysSinceUpdate = (DateTime.UtcNow - (order.UpdatedAt ?? order.CreatedAt)).TotalDays;
        if (daysSinceUpdate > 30)
            return BadRequest("Cannot return order after 30 days from delivery.");

        // Restore stock
        foreach (var item in order.Items)
        {
            await _productClient.IncrementStockAsync(item.ProductId, item.Quantity);
        }

        // Request refund
        await _paymentClient.RequestRefundAsync(order.Id);

        order.Status = "Returned";
        order.ReturnedAt = DateTime.UtcNow;
        order.ReturnReason = request?.Reason;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationClient.SendNotificationAsync(
            order.BuyerId,
            "OrderReturned",
            "Return accepted",
            $"Your order #{order.Id} has been returned. Refund is being processed.");

        // Notify sellers
        var returnSellerIds = order.Items.Select(i => i.SellerId).Distinct();
        foreach (var sellerId in returnSellerIds)
        {
            await _notificationClient.SendNotificationAsync(
                sellerId,
                "OrderReturned",
                $"Order #{order.Id} returned",
                $"Order #{order.Id} has been returned by the buyer. Stock restored.");
        }

        return Ok(order);
    }

    /// <summary>
    /// General order dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var orders = await _context.Orders.Include(o => o.Items).ToListAsync();
        var now = DateTime.UtcNow;

        return Ok(new
        {
            TotalOrders = orders.Count,
            ByStatus = orders.GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count),
            AverageTicket = orders.Count > 0 ? Math.Round(orders.Average(o => o.Total), 2) : 0,
            TotalRevenue = Math.Round(orders.Where(o => o.Status != "Cancelled" && o.Status != "Returned").Sum(o => o.Total), 2),
            CancelledLast7Days = orders.Count(o => o.Status == "Cancelled" && o.CancelledAt >= now.AddDays(-7)),
            CancelledLast30Days = orders.Count(o => o.Status == "Cancelled" && o.CancelledAt >= now.AddDays(-30)),
            ReturnedLast30Days = orders.Count(o => o.Status == "Returned" && o.ReturnedAt >= now.AddDays(-30)),
            CancellationRate = orders.Count > 0 ? Math.Round((double)orders.Count(o => o.Status == "Cancelled") / orders.Count * 100, 2) : 0,
            ReturnRate = orders.Count > 0 ? Math.Round((double)orders.Count(o => o.Status == "Returned") / orders.Count * 100, 2) : 0
        });
    }

    /// <summary>
    /// Buyer-specific dashboard
    /// </summary>
    [HttpGet("buyer/{buyerId}/dashboard")]
    public async Task<IActionResult> BuyerDashboard(int buyerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.BuyerId == buyerId)
            .ToListAsync();

        var lastOrder = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault();

        return Ok(new
        {
            TotalOrders = orders.Count,
            TotalSpent = Math.Round(orders.Where(o => o.Status != "Cancelled" && o.Status != "Returned").Sum(o => o.Total), 2),
            ByStatus = orders.GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }),
            LastOrder = lastOrder != null ? new { lastOrder.Id, lastOrder.Status, lastOrder.Total, lastOrder.CreatedAt } : null
        });
    }

    /// <summary>
    /// Seller-specific dashboard (based on OrderItems where SellerId matches)
    /// </summary>
    [HttpGet("seller/{sellerId}/dashboard")]
    public async Task<IActionResult> SellerDashboard(int sellerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Items.Any(i => i.SellerId == sellerId))
            .ToListAsync();

        var sellerItems = orders.SelectMany(o => o.Items.Where(i => i.SellerId == sellerId)).ToList();

        return Ok(new
        {
            TotalOrders = orders.Count,
            TotalItemsSold = sellerItems.Sum(i => i.Quantity),
            TotalRevenue = Math.Round(sellerItems.Sum(i => i.Subtotal), 2),
            ByStatus = orders.GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() }),
            AverageOrderValue = orders.Count > 0 ? Math.Round(sellerItems.Sum(i => i.Subtotal) / orders.Count, 2) : 0
        });
    }
}
