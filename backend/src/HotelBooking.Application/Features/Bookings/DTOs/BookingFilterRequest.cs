using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Bookings.DTOs;

public sealed record BookingFilterRequest(
    BookingStatus? Status = null,
    string? GuestName = null,
    string? BookingCode = null,
    string? RoomNumber = null,
    DateOnly? CheckInFrom = null,
    DateOnly? CheckInTo = null,
    int Page = 1,
    int PageSize = 20);
