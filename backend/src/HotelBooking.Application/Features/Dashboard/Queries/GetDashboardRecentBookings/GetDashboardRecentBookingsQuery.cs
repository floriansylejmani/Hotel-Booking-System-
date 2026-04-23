using HotelBooking.Application.Features.Bookings.DTOs;
using MediatR;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardRecentBookings;

public sealed record GetDashboardRecentBookingsQuery(int Count = 5)
    : IRequest<IReadOnlyList<BookingResponse>>;
