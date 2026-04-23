using MediatR;

namespace HotelBooking.Application.Common.Events;

public record PaymentCompletedEvent(
    Guid PaymentId,
    Guid BookingId,
    string GuestEmail,
    string GuestName,
    decimal Amount,
    string InvoiceNumber) : INotification;
