namespace HotelBooking.Application.Features.Staff.DTOs;

public sealed record StaffResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

public sealed record StaffPagedResult(
    IReadOnlyList<StaffResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record HousekeeperOption(Guid Id, string FullName);
