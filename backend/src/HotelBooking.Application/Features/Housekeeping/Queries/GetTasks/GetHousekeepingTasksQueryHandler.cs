using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Housekeeping.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Housekeeping.Queries.GetTasks;

public class GetHousekeepingTasksQueryHandler(IAppDbContext db)
    : IRequestHandler<GetHousekeepingTasksQuery, GetHousekeepingTasksResult>
{
    public async Task<GetHousekeepingTasksResult> Handle(
        GetHousekeepingTasksQuery request, CancellationToken cancellationToken)
    {
        var query = db.HousekeepingTasks
            .AsNoTracking()
            .Include(t => t.Room)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == request.AssignedToUserId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new HousekeepingTaskDto(
                t.Id,
                t.Room.RoomNumber,
                t.Status.ToString(),
                t.AssignedTo != null ? t.AssignedTo.FullName : null,
                t.Notes,
                t.LastCleanedAt))
            .ToListAsync(cancellationToken);

        return new GetHousekeepingTasksResult(tasks, totalCount, request.Page, request.PageSize);
    }
}
