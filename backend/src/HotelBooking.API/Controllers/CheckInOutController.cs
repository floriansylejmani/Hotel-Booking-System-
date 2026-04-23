using HotelBooking.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "ReceptionistOrAbove")]
public class CheckInOutController(ICheckInOutService checkInOutService) : BaseController
{
    [HttpPut("/api/checkin/{bookingId:guid}")]
    public async Task<IActionResult> CheckIn(Guid bookingId, CancellationToken ct)
    {
        var result = await checkInOutService.CheckInAsync(bookingId, ct);
        return Ok(result);
    }

    [HttpPut("/api/checkout/{bookingId:guid}")]
    public async Task<IActionResult> CheckOut(Guid bookingId, CancellationToken ct)
    {
        var result = await checkInOutService.CheckOutAsync(bookingId, ct);
        return Ok(result);
    }
}
