using HotelBooking.Application.Features.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "StaffOrAbove")]
public class RolesController(IRoleService roleService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await roleService.GetAllAsync(ct);
        return Ok(roles);
    }
}
