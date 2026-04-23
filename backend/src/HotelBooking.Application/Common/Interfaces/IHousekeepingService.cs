using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Common.Interfaces;

public interface IHousekeepingService
{
    Task<IReadOnlyList<HousekeepingTaskDto>> GetTasksAsync(CancellationToken cancellationToken = default);
    Task<HousekeepingTaskDto> UpdateStatusAsync(Guid taskId, HousekeepingStatus status, string? notes = null, CancellationToken cancellationToken = default);
    Task<HousekeepingTaskDto> AssignTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}