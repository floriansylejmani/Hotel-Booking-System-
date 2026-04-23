using HotelBooking.Application.Common.Models;
using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtToken GenerateToken(User user);
}
