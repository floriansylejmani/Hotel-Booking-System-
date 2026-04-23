using HotelBooking.Application.Features.Staff;
using HotelBooking.Application.Features.Staff.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "ManagerOrAdmin")]
public class StaffController(IStaffService staffService) : BaseController
{
    [HttpGet("housekeepers")]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<IActionResult> GetHousekeepers(CancellationToken ct)
    {
        var result = await staffService.GetAssignableHousekeepersAsync(ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? roleName,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new StaffFilterRequest(roleName, isActive, search, page, pageSize);
        var result = await staffService.GetAllAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await staffService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request, CancellationToken ct)
    {
        var result = await staffService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var result = await staffService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await staffService.SetActiveAsync(id, false, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await staffService.SetActiveAsync(id, true, ct);
        return Ok(result);
    }
}
