using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardActivityFeed;

public sealed class GetDashboardActivityFeedQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDashboardActivityFeedQuery, DashboardActivityFeedResponse>
{
    public async Task<DashboardActivityFeedResponse> Handle(
        GetDashboardActivityFeedQuery request,
        CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count, 1, 20);

        var items = await db.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new ActivityFeedItem(
                n.Id,
                n.Title,
                n.Message,
                n.Type.ToString(),
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return new DashboardActivityFeedResponse(items);
    }
}
