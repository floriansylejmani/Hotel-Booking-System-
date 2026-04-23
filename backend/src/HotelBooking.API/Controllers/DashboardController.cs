using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardActivityFeed;
using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardOccupancy;
using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardRecentBookings;
using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "ReceptionistOrAbove")]
public class DashboardController(ISender sender) : BaseController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await sender.Send(new GetDashboardSummaryQuery(GetCurrentUserId()), ct);
        return Ok(result);
    }

    [HttpGet("recent-bookings")]
    public async Task<IActionResult> GetRecentBookings(
        [FromQuery] int count = 5,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetDashboardRecentBookingsQuery(count), ct);
        return Ok(result);
    }

    [HttpGet("activity-feed")]
    public async Task<IActionResult> GetActivityFeed(
        [FromQuery] int count = 6,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetDashboardActivityFeedQuery(count), ct);
        return Ok(result);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy(CancellationToken ct)
    {
        var result = await sender.Send(new GetDashboardOccupancyQuery(), ct);
        return Ok(result);
    }
}
