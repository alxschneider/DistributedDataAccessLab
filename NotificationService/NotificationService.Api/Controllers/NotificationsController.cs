using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Data;
using NotificationService.Api.Dtos;
using NotificationService.Api.Models;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;

    public NotificationsController(NotificationDbContext context)
    {
        _context = context;
    }

    //!!!IMporant: In a bigger or prod enviroment, its normal to have the business logic in a separate service layer, and the controller just calls that service.
    //  For simplicity, we put all logic in the controller here.!!!

    private static NotificationResponseDto MapToDto(Notification n) => new(
        n.Id, n.UserId, n.Type, n.Title, n.Message, n.IsRead, n.CreatedAt
    );

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Running", Service = "NotificationService" });
    }

    /// <summary>
    /// Get notifications for a user (most recent first, paginated)
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new NotificationPageDto(
            total, page, pageSize,
            notifications.Select(MapToDto).ToList()
        ));
    }

    /// <summary>
    /// Get unread count for a user
    /// </summary>
    [HttpGet("{userId}/unread-count")]
    public async Task<IActionResult> UnreadCount(int userId)
    {
        var count = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return Ok(new UnreadCountDto(userId, count));
    }

    /// <summary>
    /// Create a notification (called by other services via HTTP)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByUser), new { userId = notification.UserId }, MapToDto(notification));
    }

    /// <summary>
    /// Mark a single notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok(MapToDto(notification));
    }

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    [HttpPut("{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(int userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
            n.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok(new MarkReadResultDto(unread.Count));
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Dashboard: total by type, unread per user
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var notifications = await _context.Notifications.ToListAsync();

        return Ok(new NotificationDashboardDto(
            notifications.Count,
            notifications.Count(n => !n.IsRead),
            notifications.GroupBy(n => n.Type)
                .Select(g => new TypeCountDto(g.Key, g.Count()))
                .OrderByDescending(x => x.Count),
            notifications.Where(n => !n.IsRead)
                .GroupBy(n => n.UserId)
                .Select(g => new UserUnreadDto(g.Key, g.Count()))
                .OrderByDescending(x => x.UnreadCount)
                .Take(10)
        ));
    }
}
