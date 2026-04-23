namespace HotelBooking.Application.Features.Bookings.DTOs;

public sealed record BookingResponse(
    Guid Id,
    string BookingCode,
    Guid UserId,
    string GuestName,
    string GuestEmail,
    Guid RoomId,
    string RoomNumber,
    string RoomType,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    decimal TotalAmount,
    string PaymentMethod,
    string Status,
    int Nights,
    DateTime CreatedAt);
