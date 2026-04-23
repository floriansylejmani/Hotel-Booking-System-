using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Bookings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardRecentBookings;

public sealed class GetDashboardRecentBookingsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDashboardRecentBookingsQuery, IReadOnlyList<BookingResponse>>
{
    public async Task<IReadOnlyList<BookingResponse>> Handle(
        GetDashboardRecentBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count, 1, 20);

        return await db.Bookings
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .Select(b => new BookingResponse(
                b.Id,
                b.BookingCode,
                b.UserId,
                b.User.FullName,
                b.User.Email,
                b.RoomId,
                b.Room.RoomNumber,
                b.Room.Type.ToString(),
                b.CheckInDate,
                b.CheckOutDate,
                b.TotalAmount,
                b.PaymentMethod.ToString(),
                b.Status.ToString(),
                b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber,
                b.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
