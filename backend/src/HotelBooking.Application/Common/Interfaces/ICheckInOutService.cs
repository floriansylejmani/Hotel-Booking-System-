using HotelBooking.Application.Features.CheckInOut.DTOs;

namespace HotelBooking.Application.Common.Interfaces;

public interface ICheckInOutService
{
    Task<CheckInOutResponse> CheckInAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<CheckInOutResponse> CheckOutAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
