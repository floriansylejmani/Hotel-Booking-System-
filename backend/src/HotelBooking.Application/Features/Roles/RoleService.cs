using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Roles.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Roles;

public sealed class RoleService(IAppDbContext db) : IRoleService
{
    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponse(r.Id, r.Name))
            .ToListAsync(cancellationToken);
    }
}
