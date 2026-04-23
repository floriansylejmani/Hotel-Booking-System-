namespace HotelBooking.Application.Features.Dashboard.DTOs;

public sealed record DashboardOccupancyResponse(
    decimal OccupancyPercentage,
    IReadOnlyList<RoomOccupancyItem> Rooms);

public sealed record RoomOccupancyItem(
    Guid RoomId,
    string RoomNumber,
    string Type,
    string Status,
    decimal PricePerNight,
    int FloorNumber,
    int BedCount,
    string[]? Amenities,
    string? GuestName);
