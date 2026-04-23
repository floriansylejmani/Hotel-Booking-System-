using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Notifications.EventHandlers;

public class PaymentCompletedNotificationHandler(
    IEmailService emailService,
    ILogger<PaymentCompletedNotificationHandler> logger)
    : INotificationHandler<PaymentCompletedEvent>
{
    public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "NOTIFICATION [PaymentCompleted] PaymentId={PaymentId} BookingId={BookingId} " +
            "Guest={GuestName} <{GuestEmail}> Amount={Amount:C} Invoice={InvoiceNumber}",
            notification.PaymentId, notification.BookingId, notification.GuestName,
            notification.GuestEmail, notification.Amount, notification.InvoiceNumber);

        var subject = $"Payment Receipt — Invoice {notification.InvoiceNumber}";
        var body = $"""
            <p>Dear {notification.GuestName},</p>
            <p>Your payment has been received. Thank you!</p>
            <ul>
              <li><strong>Invoice number:</strong> {notification.InvoiceNumber}</li>
              <li><strong>Amount paid:</strong> {notification.Amount:C}</li>
            </ul>
            <p>Please keep this email as your receipt.</p>
            """;

        await emailService.SendAsync(notification.GuestEmail, subject, body, cancellationToken);
    }
}
