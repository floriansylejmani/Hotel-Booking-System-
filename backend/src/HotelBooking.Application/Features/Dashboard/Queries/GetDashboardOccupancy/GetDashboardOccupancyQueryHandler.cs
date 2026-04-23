using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Dashboard.DTOs;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardOccupancy;

public sealed class GetDashboardOccupancyQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDashboardOccupancyQuery, DashboardOccupancyResponse>
{
    public async Task<DashboardOccupancyResponse> Handle(
        GetDashboardOccupancyQuery request,
        CancellationToken cancellationToken)
    {
        var rooms = await db.Rooms
            .AsNoTracking()
            .OrderBy(r => r.FloorNumber)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync(cancellationToken);

        // A room may have both an Active and a Confirmed booking at the same time
        // (current stay + next reservation), so we group by RoomId and prefer Active.
        var guestByRoomId = (await db.Bookings
                .AsNoTracking()
                .Where(b => b.Status == BookingStatus.Active || b.Status == BookingStatus.Confirmed)
                .Select(b => new { b.RoomId, b.User.FullName, b.Status })
                .ToListAsync(cancellationToken))
            .GroupBy(b => b.RoomId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(b => b.Status == BookingStatus.Active).First().FullName);

        var totalRooms = rooms.Count;
        var occupiedCount = rooms.Count(r => r.Status == RoomStatus.Occupied);
        var occupancyPct = totalRooms > 0
            ? Math.Round((decimal)occupiedCount / totalRooms * 100, 1)
            : 0m;

        var items = rooms.Select(r => new RoomOccupancyItem(
            r.Id,
            r.RoomNumber,
            r.Type.ToString(),
            r.Status.ToString(),
            r.PricePerNight,
            r.FloorNumber,
            r.BedCount,
            r.Amenities,
            guestByRoomId.GetValueOrDefault(r.Id))).ToList();

        return new DashboardOccupancyResponse(occupancyPct, items);
    }
}
