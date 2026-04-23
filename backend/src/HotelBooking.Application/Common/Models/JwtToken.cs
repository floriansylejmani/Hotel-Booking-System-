namespace HotelBooking.Application.Common.Models;

public sealed record JwtToken(string Token, DateTime ExpiresAt);
