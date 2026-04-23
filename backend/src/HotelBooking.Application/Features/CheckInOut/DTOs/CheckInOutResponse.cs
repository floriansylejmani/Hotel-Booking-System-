namespace HotelBooking.Application.Features.CheckInOut.DTOs;

public sealed record CheckInOutResponse(
    Guid BookingId,
    string BookingCode,
    string BookingStatus,
    string RoomNumber,
    string RoomStatus,
    DateTime Timestamp,
    string Message);
