using HotelBooking.Application.Features.Notifications.DTOs;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Notifications;

public interface INotificationService
{
    Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        CancellationToken cancellationToken = default);

    Task<NotificationsPagedResult> GetForUserAsync(
        Guid userId,
        bool unreadOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
