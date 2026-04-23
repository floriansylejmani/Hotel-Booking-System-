namespace HotelBooking.Application.Features.Housekeeping.DTOs;

public record HousekeepingTaskDto(
    Guid Id,
    string RoomNumber,
    string Status,
    string? AssignedTo,
    string Notes,
    DateTime? LastCleanedAt);
