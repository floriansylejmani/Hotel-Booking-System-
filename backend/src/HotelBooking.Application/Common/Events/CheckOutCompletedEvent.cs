using MediatR;

namespace HotelBooking.Application.Common.Events;

public sealed record CheckOutCompletedEvent(
    Guid BookingId,
    Guid UserId,
    string GuestEmail,
    string GuestName,
    string BookingCode,
    string RoomNumber,
    DateTime CompletedAt) : INotification;
