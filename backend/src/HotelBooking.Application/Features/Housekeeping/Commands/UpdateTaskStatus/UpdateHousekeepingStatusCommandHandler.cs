using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Housekeeping.Commands.UpdateTaskStatus;

public class UpdateHousekeepingStatusCommandHandler(IAppDbContext db, IAuditService auditService)
    : IRequestHandler<UpdateHousekeepingStatusCommand, HousekeepingTaskDto>
{
    private static readonly Dictionary<HousekeepingStatus, HashSet<HousekeepingStatus>> ValidTransitions = new()
    {
        [HousekeepingStatus.Dirty] = [HousekeepingStatus.InProgress, HousekeepingStatus.Maintenance],
        [HousekeepingStatus.InProgress] = [HousekeepingStatus.Clean, HousekeepingStatus.Dirty, HousekeepingStatus.Maintenance],
        [HousekeepingStatus.Clean] = [HousekeepingStatus.Dirty, HousekeepingStatus.Maintenance],
        [HousekeepingStatus.Maintenance] = [HousekeepingStatus.Dirty, HousekeepingStatus.InProgress],
    };

    public async Task<HousekeepingTaskDto> Handle(
        UpdateHousekeepingStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await db.HousekeepingTasks
            .Include(t => t.Room)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException("HousekeepingTask", request.TaskId);

        if (!ValidTransitions.TryGetValue(task.Status, out var allowed) ||
            !allowed.Contains(request.Status))
        {
            throw new BadRequestException(
                $"Cannot transition housekeeping task from '{task.Status}' to '{request.Status}'.");
        }

        task.Status = request.Status;

        if (request.Notes is not null)
            task.Notes = request.Notes;

        switch (request.Status)
        {
            case HousekeepingStatus.Clean:
                task.LastCleanedAt = DateTime.UtcNow;
                task.Room.Status = RoomStatus.Available;
                break;
            case HousekeepingStatus.Dirty:
                task.Room.Status = RoomStatus.Cleaning;
                break;
            case HousekeepingStatus.InProgress:
                break;
            case HousekeepingStatus.Maintenance:
                task.Room.Status = RoomStatus.Maintenance;
                break;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("HousekeepingTask", task.Id, "StatusUpdated", null,
            $"Room={task.Room.RoomNumber} Status={task.Status}",
            cancellationToken);

        return new HousekeepingTaskDto(
            task.Id, task.Room.RoomNumber,
            task.Status.ToString(),
            task.AssignedTo is not null ? task.AssignedTo.FullName : null,
            task.Notes, task.LastCleanedAt);
    }
}
