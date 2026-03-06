using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;
using PaymentService.Api.Models;
using PaymentService.Api.Services;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly IOrderClient _orderClient;
    private readonly INotificationClient _notificationClient;
    private static readonly Random _random = new();

    public PaymentsController(
        PaymentDbContext context,
        IOrderClient orderClient,
        INotificationClient notificationClient)
    {
        _context = context;
        _orderClient = orderClient;
        _notificationClient = notificationClient;
    }

    //!!!IMporant: In a bigger or prod enviroment, its normal to have the business logic in a separate service layer, and the controller just calls that service.
    //  For simplicity, we put all logic in the controller here.!!!

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? orderId,
        [FromQuery] int? buyerId,
        [FromQuery] string? status)
    {
        var query = _context.Payments.AsQueryable();

        if (orderId.HasValue)
            query = query.Where(p => p.OrderId == orderId.Value);

        if (buyerId.HasValue)
            query = query.Where(p => p.BuyerId == buyerId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        return Ok(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    /// <summary>
    /// Create a payment for an order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        var validMethods = new[] { "CreditCard", "Pix", "Boleto" };
        if (!validMethods.Contains(request.Method))
            return BadRequest($"Invalid method. Use: {string.Join(", ", validMethods)}");

        // Check if payment already exists for this order
        var existing = await _context.Payments
            .AnyAsync(p => p.OrderId == request.OrderId && (p.Status == "Pending" || p.Status == "Approved"));
        if (existing)
            return BadRequest("A payment already exists for this order.");

        var payment = new Payment
        {
            OrderId = request.OrderId,
            BuyerId = request.BuyerId,
            Amount = request.Amount,
            Method = request.Method,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    /// <summary>
    /// Process payment — MOCK: 90% approval, 10% rejection, 0-2s delay
    /// </summary>
    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        if (payment.Status != "Pending")
            return BadRequest($"Cannot process payment with status '{payment.Status}'.");

        payment.Status = "Processing";
        await _context.SaveChangesAsync();

        // Simulate processing delay (0-2 seconds)
        var delay = _random.Next(0, 2000);
        await Task.Delay(delay);

        // Mock: 90% approval rate
        var approved = _random.Next(1, 101) <= 90;

        payment.Status = approved ? "Approved" : "Rejected";
        payment.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Update order status
        if (approved)
        {
            await _orderClient.UpdateOrderStatusAsync(payment.OrderId, "Paid");
            await _notificationClient.SendNotificationAsync(
                payment.BuyerId,
                "PaymentApproved",
                "Payment approved",
                $"Your payment of $ {payment.Amount:F2} for order #{payment.OrderId} has been approved via {payment.Method}.");
        }
        else
        {
            await _notificationClient.SendNotificationAsync(
                payment.BuyerId,
                "PaymentRejected",
                "Payment rejected",
                $"Your payment of $ {payment.Amount:F2} for order #{payment.OrderId} was rejected. Please try again.");
        }

        return Ok(new
        {
            payment.Id,
            payment.OrderId,
            payment.Status,
            payment.Method,
            payment.Amount,
            payment.ProcessedAt,
            SimulatedDelayMs = delay
        });
    }

    /// <summary>
    /// Refund a payment (for cancellations/returns)
    /// </summary>
    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        if (payment.Status != "Approved")
            return BadRequest($"Cannot refund payment with status '{payment.Status}'. Only 'Approved' payments can be refunded.");

        payment.Status = "Refunded";
        payment.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notificationClient.SendNotificationAsync(
            payment.BuyerId,
            "PaymentRefunded",
            "Refund processed",
            $"Your refund of $ {payment.Amount:F2} for order #{payment.OrderId} has been processed.");

        return Ok(payment);
    }

    /// <summary>
    /// Payment dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var payments = await _context.Payments.ToListAsync();

        var approved = payments.Where(p => p.Status == "Approved").ToList();
        var refunded = payments.Where(p => p.Status == "Refunded").ToList();

        return Ok(new
        {
            TotalPayments = payments.Count,
            TotalRevenue = Math.Round(approved.Sum(p => p.Amount), 2),
            TotalRefunded = Math.Round(refunded.Sum(p => p.Amount), 2),
            ByStatus = payments.GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count(), Total = Math.Round(g.Sum(p => p.Amount), 2) }),
            ByMethod = payments.GroupBy(p => p.Method)
                .Select(g => new { Method = g.Key, Count = g.Count(), Total = Math.Round(g.Sum(p => p.Amount), 2) }),
            ApprovalRate = payments.Count(p => p.Status == "Approved" || p.Status == "Rejected") > 0
                ? Math.Round((double)payments.Count(p => p.Status == "Approved") / payments.Count(p => p.Status == "Approved" || p.Status == "Rejected") * 100, 2)
                : 0
        });
    }
}

public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "CreditCard";
}
