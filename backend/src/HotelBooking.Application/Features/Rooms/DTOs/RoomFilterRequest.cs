using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Rooms.DTOs;

public sealed record RoomFilterRequest(
    RoomType? Type = null,
    RoomStatus? Status = null,
    int? Floor = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);
