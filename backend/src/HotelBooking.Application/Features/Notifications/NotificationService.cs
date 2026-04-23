using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Notifications.DTOs;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Notifications;

public sealed class NotificationService(IAppDbContext db) : INotificationService
{
    private const int MaxPageSize = 100;
    private const int MaxTitleLength = 150;
    private const int MaxMessageLength = 1000;

    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new BadRequestException("Notification user id is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new BadRequestException("Notification title is required.");

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > MaxTitleLength)
            throw new BadRequestException($"Notification title cannot exceed {MaxTitleLength} characters.");

        if (string.IsNullOrWhiteSpace(message))
            throw new BadRequestException("Notification message is required.");

        var normalizedMessage = message.Trim();
        if (normalizedMessage.Length > MaxMessageLength)
            throw new BadRequestException($"Notification message cannot exceed {MaxMessageLength} characters.");

        var notification = new Notification
        {
            UserId = userId,
            Title = normalizedTitle,
            Message = normalizedMessage,
            Type = type,
            IsRead = false
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationsPagedResult> GetForUserAsync(
        Guid userId,
        bool unreadOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

        var query = db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var unreadCount = await db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .Select(n => new NotificationResponse(
                n.Id,
                n.Title,
                n.Message,
                n.Type.ToString(),
                n.IsRead,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return new NotificationsPagedResult(items, unreadCount, totalCount, safePageNumber, safePageSize);
    }

    public async Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken)
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new ForbiddenException("You can only modify your own notifications.");

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.IsRead, true),
                cancellationToken);
    }
}
