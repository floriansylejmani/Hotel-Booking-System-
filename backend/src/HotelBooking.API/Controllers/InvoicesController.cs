using HotelBooking.Application.Features.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[Authorize(Policy = "ReceptionistOrAbove")]
public class InvoicesController(IInvoiceService invoiceService) : BaseController
{
    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetInvoice(Guid bookingId, CancellationToken ct)
    {
        var result = await invoiceService.GetByBookingIdAsync(bookingId, ct);
        return Ok(result);
    }

    [HttpGet("{bookingId:guid}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(Guid bookingId, CancellationToken ct)
    {
        var invoice = await invoiceService.GetByBookingIdAsync(bookingId, ct);
        var pdfBytes = await invoiceService.GetPdfByBookingIdAsync(bookingId, ct);
        return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
