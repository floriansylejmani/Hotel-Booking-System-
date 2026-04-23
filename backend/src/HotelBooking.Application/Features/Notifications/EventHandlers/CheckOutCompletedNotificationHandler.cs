using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Notifications.EventHandlers;

public sealed class CheckOutCompletedNotificationHandler(
    IEmailService emailService,
    ILogger<CheckOutCompletedNotificationHandler> logger)
    : INotificationHandler<CheckOutCompletedEvent>
{
    public async Task Handle(CheckOutCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NOTIFICATION [CheckOutCompleted] BookingId={BookingId} Guest={GuestName} <{GuestEmail}> " +
            "BookingCode={BookingCode} Room={RoomNumber} CompletedAt={CompletedAt}",
            notification.BookingId,
            notification.GuestName,
            notification.GuestEmail,
            notification.BookingCode,
            notification.RoomNumber,
            notification.CompletedAt);

        var subject = $"Thank You for Staying with Us — Room {notification.RoomNumber}";
        var body = $"""
            <p>Dear {notification.GuestName},</p>
            <p>You have successfully checked out. We hope you enjoyed your stay!</p>
            <ul>
              <li><strong>Room:</strong> {notification.RoomNumber}</li>
              <li><strong>Booking code:</strong> {notification.BookingCode}</li>
              <li><strong>Check-out time:</strong> {notification.CompletedAt:MMMM d, yyyy h:mm tt}</li>
            </ul>
            <p>We hope to see you again soon.</p>
            """;

        await emailService.SendAsync(notification.GuestEmail, subject, body, cancellationToken);
    }
}
