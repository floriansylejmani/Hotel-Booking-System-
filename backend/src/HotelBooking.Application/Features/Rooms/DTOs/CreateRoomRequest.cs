using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Rooms.DTOs;

public sealed record CreateRoomRequest(
    string RoomNumber,
    int FloorNumber,
    RoomType Type,
    decimal PricePerNight,
    int BedCount,
    string[] Amenities);
