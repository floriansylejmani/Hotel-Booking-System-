namespace HotelBooking.Application.Features.Notifications.DTOs;

public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt);

public sealed record NotificationsPagedResult(
    IReadOnlyList<NotificationResponse> Items,
    int UnreadCount,
    int TotalCount,
    int PageNumber,
    int PageSize);
