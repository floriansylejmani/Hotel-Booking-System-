namespace HotelBooking.Application.Features.Bookings.DTOs;

public sealed record BookingsPagedResult(
    IReadOnlyList<BookingResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
