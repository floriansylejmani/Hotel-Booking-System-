using HotelBooking.Application.Features.Payments;
using HotelBooking.Application.Features.Payments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

public class PaymentsController(IPaymentService paymentService) : BaseController
{
    [HttpPost]
    [Authorize(Policy = "ReceptionistOrAbove")]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        var result = await paymentService.ProcessAsync(request, ct);
        return Ok(result);
    }
}
