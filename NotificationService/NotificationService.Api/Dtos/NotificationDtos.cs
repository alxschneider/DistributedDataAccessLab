namespace NotificationService.Api.Dtos;

// Request DTOs
public record CreateNotificationDto(
    int UserId,
    string Type,
    string Title,
    string Message
);

// Response DTOs
public record NotificationResponseDto(
    int Id,
    int UserId,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);

public record NotificationPageDto(
    int Total,
    int Page,
    int PageSize,
    List<NotificationResponseDto> Items
);

public record UnreadCountDto(int UserId, int UnreadCount);

public record NotificationDashboardDto(
    int TotalNotifications,
    int TotalUnread,
    IEnumerable<TypeCountDto> ByType,
    IEnumerable<UserUnreadDto> TopUsersByUnread
);

public record TypeCountDto(string Type, int Count);
public record UserUnreadDto(int UserId, int UnreadCount);
public record MarkReadResultDto(int MarkedAsRead);
