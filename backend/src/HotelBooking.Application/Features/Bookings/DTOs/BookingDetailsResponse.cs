namespace HotelBooking.Application.Features.Bookings.DTOs;

public sealed record BookingDetailsResponse(
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
    int GuestCount,
    string? SpecialRequests,
    DateTime? ActualCheckIn,
    DateTime? ActualCheckOut,
    DateTime CreatedAt);
