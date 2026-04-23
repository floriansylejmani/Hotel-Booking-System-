using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Bookings.DTOs;

public sealed record CreateBookingRequest(
    Guid? UserId,
    Guid RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    PaymentMethod PaymentMethod,
    int GuestCount = 1,
    string? SpecialRequests = null);
