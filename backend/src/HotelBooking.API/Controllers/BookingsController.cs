using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize]
public class BookingsController(IBookingService bookingService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<IActionResult> GetBookings(
        [FromQuery] BookingStatus? status,
        [FromQuery] string? guestName,
        [FromQuery] string? bookingCode,
        [FromQuery] string? roomNumber,
        [FromQuery] DateOnly? checkInFrom,
        [FromQuery] DateOnly? checkInTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new BookingFilterRequest(status, guestName, bookingCode, roomNumber, checkInFrom, checkInTo, page, pageSize);
        var result = await bookingService.GetAllAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<IActionResult> GetBooking(Guid id, CancellationToken ct)
    {
        var result = await bookingService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken ct)
    {
        var callerId = GetCurrentUserId();
        var result = await bookingService.CreateAsync(request, callerId, ct);
        return CreatedAtAction(nameof(GetBooking), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken ct)
    {
        var callerId = GetCurrentUserId();
        var callerRole = GetCurrentUserRole();
        var result = await bookingService.CancelAsync(id, callerId, callerRole, ct);
        return Ok(result);
    }
}
