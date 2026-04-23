using HotelBooking.Application.Features.Bookings.DTOs;

namespace HotelBooking.Application.Common.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, Guid callerId, CancellationToken cancellationToken = default);
    Task<BookingsPagedResult> GetAllAsync(BookingFilterRequest filter, CancellationToken cancellationToken = default);
    Task<BookingDetailsResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BookingResponse> CancelAsync(Guid id, Guid callerId, string callerRole, CancellationToken cancellationToken = default);
}
