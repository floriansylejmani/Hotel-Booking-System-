using HotelBooking.Application.Features.Payments.DTOs;

namespace HotelBooking.Application.Features.Payments;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default);
}
