namespace HotelBooking.Application.Features.Rooms.DTOs;

public sealed record RoomsPagedResult(
    IReadOnlyList<RoomResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
