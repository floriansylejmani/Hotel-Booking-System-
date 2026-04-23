namespace HotelBooking.Application.Features.Rooms.DTOs;

public sealed record RoomResponse(
    Guid Id,
    string RoomNumber,
    int FloorNumber,
    string Type,
    string Status,
    decimal PricePerNight,
    int BedCount,
    string[] Amenities,
    DateTime CreatedAt);
