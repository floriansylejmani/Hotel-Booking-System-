namespace HotelBooking.Application.Features.Auth.DTOs;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string? RoleName);
