using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Dashboard.DTOs;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    public async Task<DashboardSummaryResponse> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1,
            0, 0, 0, DateTimeKind.Utc);

        var roomStatusCounts = await db.Rooms
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var bookingStatusCounts = await db.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var monthlyRevenue = await db.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= startOfMonth)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken);

        var unreadCount = await db.Notifications
            .CountAsync(n => n.UserId == request.CallerId && !n.IsRead, cancellationToken);

        var roomCounts = roomStatusCounts
            .ToDictionary(x => x.Status, x => x.Count);

        var bookingCounts = bookingStatusCounts
            .ToDictionary(x => x.Status, x => x.Count);

        var totalRooms = roomCounts.Values.Sum();

        return new DashboardSummaryResponse(
            TotalRooms: totalRooms,
            AvailableRooms: roomCounts.GetValueOrDefault(RoomStatus.Available),
            OccupiedRooms: roomCounts.GetValueOrDefault(RoomStatus.Occupied),
            CleaningRooms: roomCounts.GetValueOrDefault(RoomStatus.Cleaning),
            MaintenanceRooms: roomCounts.GetValueOrDefault(RoomStatus.Maintenance),
            TotalBookings: bookingCounts.Values.Sum(),
            ActiveBookings: bookingCounts.GetValueOrDefault(BookingStatus.Active),
            ConfirmedBookings: bookingCounts.GetValueOrDefault(BookingStatus.Confirmed),
            MonthlyRevenue: monthlyRevenue ?? 0m,
            UnreadNotificationsCount: unreadCount);
    }
}
