using MediatR;

namespace HotelBooking.Application.Common.Events;

public record BookingCreatedEvent(
    Guid BookingId,
    Guid UserId,
    string GuestEmail,
    string GuestName,
    string RoomNumber,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    decimal TotalAmount) : INotification;
