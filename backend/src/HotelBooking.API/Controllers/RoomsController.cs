using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "StaffOrAbove")]
public class RoomsController(IRoomService roomService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetRooms(
        [FromQuery] RoomType? type,
        [FromQuery] RoomStatus? status,
        [FromQuery] int? floor,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new RoomFilterRequest(type, status, floor, search, page, pageSize);
        var result = await roomService.GetAllAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken ct)
    {
        var result = await roomService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> CreateRoom(
        [FromBody] CreateRoomRequest request,
        CancellationToken ct)
    {
        var result = await roomService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetRoom), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> UpdateRoom(
        Guid id,
        [FromBody] UpdateRoomRequest request,
        CancellationToken ct)
    {
        var result = await roomService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken ct)
    {
        await roomService.DeleteAsync(id, ct);
        return NoContent();
    }
}
