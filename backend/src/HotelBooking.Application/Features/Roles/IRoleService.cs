using HotelBooking.Application.Features.Roles.DTOs;

namespace HotelBooking.Application.Features.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
