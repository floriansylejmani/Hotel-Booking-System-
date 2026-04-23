using HotelBooking.Application.Features.Payments.DTOs;

namespace HotelBooking.Application.Features.Payments;

public interface IInvoiceService
{
    Task<InvoiceResponse> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<byte[]> GetPdfByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
