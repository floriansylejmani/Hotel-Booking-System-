using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Notifications.EventHandlers;

public class BookingCreatedNotificationHandler(
    IEmailService emailService,
    ILogger<BookingCreatedNotificationHandler> logger)
    : INotificationHandler<BookingCreatedEvent>
{
    public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NOTIFICATION [BookingCreated] BookingId={BookingId} Guest={GuestName} <{GuestEmail}> " +
            "Room={RoomNumber} CheckIn={CheckIn} CheckOut={CheckOut} Total={Total:C}",
            notification.BookingId, notification.GuestName, notification.GuestEmail,
            notification.RoomNumber, notification.CheckInDate, notification.CheckOutDate,
            notification.TotalAmount);

        var subject = $"Booking Confirmation — Room {notification.RoomNumber}";
        var body = $"""
            <p>Dear {notification.GuestName},</p>
            <p>Your booking has been confirmed. Here are the details:</p>
            <ul>
              <li><strong>Room:</strong> {notification.RoomNumber}</li>
              <li><strong>Check-in:</strong> {notification.CheckInDate:MMMM d, yyyy}</li>
              <li><strong>Check-out:</strong> {notification.CheckOutDate:MMMM d, yyyy}</li>
              <li><strong>Total:</strong> {notification.TotalAmount:C}</li>
            </ul>
            <p>We look forward to welcoming you.</p>
            """;

        await emailService.SendAsync(notification.GuestEmail, subject, body, cancellationToken);
    }
}
