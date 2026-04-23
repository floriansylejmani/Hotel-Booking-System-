using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Rooms.DTOs;

public sealed record UpdateRoomRequest(
    string? RoomNumber,
    int? FloorNumber,
    RoomType? Type,
    decimal? PricePerNight,
    int? BedCount,
    string[]? Amenities,
    RoomStatus? Status);
