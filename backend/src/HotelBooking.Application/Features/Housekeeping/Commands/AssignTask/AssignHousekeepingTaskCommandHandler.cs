using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Housekeeping.Commands.AssignTask;

public class AssignHousekeepingTaskCommandHandler(IAppDbContext db, IAuditService auditService)
    : IRequestHandler<AssignHousekeepingTaskCommand, HousekeepingTaskDto>
{
    public async Task<HousekeepingTaskDto> Handle(
        AssignHousekeepingTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.HousekeepingTasks
            .Include(t => t.Room)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException("HousekeepingTask", request.TaskId);

        var staff = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.StaffUserId, cancellationToken)
            ?? throw new NotFoundException("User", request.StaffUserId);

        if (staff.Role.Name is not (RoleNames.Housekeeper or RoleNames.Manager or RoleNames.Admin))
            throw new BadRequestException("The assigned user must be a Housekeeper, Manager, or Admin.");

        task.AssignedToUserId = staff.Id;
        if (task.Status == HousekeepingStatus.Dirty)
            task.Status = HousekeepingStatus.InProgress;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("HousekeepingTask", task.Id, "Assigned", null,
            $"Room={task.Room.RoomNumber} AssignedTo={staff.FullName}",
            cancellationToken);

        return new HousekeepingTaskDto(
            task.Id, task.Room.RoomNumber,
            task.Status.ToString(), staff.FullName,
            task.Notes, task.LastCleanedAt);
    }
}
