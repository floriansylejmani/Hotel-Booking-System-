using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Notifications.EventHandlers;

public sealed class CheckInCompletedNotificationHandler(
    IEmailService emailService,
    ILogger<CheckInCompletedNotificationHandler> logger)
    : INotificationHandler<CheckInCompletedEvent>
{
    public async Task Handle(CheckInCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NOTIFICATION [CheckInCompleted] BookingId={BookingId} Guest={GuestName} <{GuestEmail}> " +
            "BookingCode={BookingCode} Room={RoomNumber} CompletedAt={CompletedAt}",
            notification.BookingId,
            notification.GuestName,
            notification.GuestEmail,
            notification.BookingCode,
            notification.RoomNumber,
            notification.CompletedAt);

        var subject = $"Welcome — You've Checked In to Room {notification.RoomNumber}";
        var body = $"""
            <p>Dear {notification.GuestName},</p>
            <p>You have successfully checked in. Enjoy your stay!</p>
            <ul>
              <li><strong>Room:</strong> {notification.RoomNumber}</li>
              <li><strong>Booking code:</strong> {notification.BookingCode}</li>
              <li><strong>Check-in time:</strong> {notification.CompletedAt:MMMM d, yyyy h:mm tt}</li>
            </ul>
            <p>If you need anything, please contact the front desk.</p>
            """;

        await emailService.SendAsync(notification.GuestEmail, subject, body, cancellationToken);
    }
}
