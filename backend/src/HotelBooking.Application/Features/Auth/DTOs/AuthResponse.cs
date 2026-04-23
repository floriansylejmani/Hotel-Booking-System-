namespace HotelBooking.Application.Features.Auth.DTOs;

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string FullName,
    string Email,
    string Role);
