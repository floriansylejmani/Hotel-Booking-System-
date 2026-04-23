using MediatR;

namespace HotelBooking.Application.Common.Events;

public record BookingCancelledEvent(
    Guid BookingId,
    string GuestEmail,
    string GuestName,
    string RoomNumber,
    DateOnly CheckInDate) : INotification;
