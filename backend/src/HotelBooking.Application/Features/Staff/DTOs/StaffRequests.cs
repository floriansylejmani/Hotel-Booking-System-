namespace HotelBooking.Application.Features.Staff.DTOs;

public sealed record CreateStaffRequest(
    string FullName,
    string Email,
    string Password,
    string RoleName);

public sealed record UpdateStaffRequest(
    string? FullName,
    string? Email,
    string? RoleName);

public sealed record StaffFilterRequest(
    string? RoleName = null,
    bool? IsActive = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);
