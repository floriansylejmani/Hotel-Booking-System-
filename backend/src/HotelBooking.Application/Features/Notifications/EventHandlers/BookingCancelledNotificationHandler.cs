using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Notifications.EventHandlers;

public class BookingCancelledNotificationHandler(
    IEmailService emailService,
    ILogger<BookingCancelledNotificationHandler> logger)
    : INotificationHandler<BookingCancelledEvent>
{
    public async Task Handle(BookingCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NOTIFICATION [BookingCancelled] BookingId={BookingId} Guest={GuestName} <{GuestEmail}> " +
            "Room={RoomNumber} CheckIn={CheckIn}",
            notification.BookingId, notification.GuestName, notification.GuestEmail,
            notification.RoomNumber, notification.CheckInDate);

        var subject = $"Booking Cancellation — Room {notification.RoomNumber}";
        var body = $"""
            <p>Dear {notification.GuestName},</p>
            <p>Your booking has been cancelled.</p>
            <ul>
              <li><strong>Room:</strong> {notification.RoomNumber}</li>
              <li><strong>Check-in date:</strong> {notification.CheckInDate:MMMM d, yyyy}</li>
            </ul>
            <p>If you did not request this cancellation, please contact the front desk immediately.</p>
            """;

        await emailService.SendAsync(notification.GuestEmail, subject, body, cancellationToken);
    }
}
