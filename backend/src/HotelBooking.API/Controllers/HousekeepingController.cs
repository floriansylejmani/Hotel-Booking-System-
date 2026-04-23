using HotelBooking.Application.Features.Housekeeping.Commands.AssignTask;
using HotelBooking.Application.Features.Housekeeping.Commands.UpdateTaskStatus;
using HotelBooking.Application.Features.Housekeeping.Queries.GetTasks;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "StaffOrAbove")]
public class HousekeepingController(ISender sender) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] HousekeepingStatus? status,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetHousekeepingTasksQuery(status, assignedToUserId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HousekeeperOrAbove")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateHousekeepingStatusCommand(id, request.Status, request.Notes), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/assign")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> AssignTask(
        Guid id, [FromBody] AssignTaskRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AssignHousekeepingTaskCommand(id, request.StaffUserId), ct);
        return Ok(result);
    }
}

public record UpdateStatusRequest(HousekeepingStatus Status, string? Notes);
public record AssignTaskRequest(Guid StaffUserId);
