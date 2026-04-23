using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Housekeeping.Commands.AssignTask;
using HotelBooking.Application.Features.Housekeeping.Commands.UpdateTaskStatus;
using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Application.Features.Housekeeping.Queries.GetTasks;
using HotelBooking.Domain.Enums;
using MediatR;

namespace HotelBooking.Application.Features.Housekeeping;

public class HousekeepingService(ISender mediator) : IHousekeepingService
{
    public async Task<IReadOnlyList<HousekeepingTaskDto>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetHousekeepingTasksQuery(), cancellationToken);
        return result.Items;
    }

    public async Task<HousekeepingTaskDto> UpdateStatusAsync(Guid taskId, HousekeepingStatus status, string? notes = null, CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new UpdateHousekeepingStatusCommand(taskId, status, notes), cancellationToken);
    }

    public async Task<HousekeepingTaskDto> AssignTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new AssignHousekeepingTaskCommand(taskId, userId), cancellationToken);
    }
}