using HotelBooking.Application.Features.Payments.DTOs;

namespace HotelBooking.Application.Common.Interfaces;

public interface IInvoicePdfService
{
    byte[] GeneratePdf(InvoiceResponse invoice);
}
