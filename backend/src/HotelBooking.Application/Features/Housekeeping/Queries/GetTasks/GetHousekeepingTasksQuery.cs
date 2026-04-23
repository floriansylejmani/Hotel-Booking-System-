using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Domain.Enums;
using MediatR;

namespace HotelBooking.Application.Features.Housekeeping.Queries.GetTasks;

public record GetHousekeepingTasksQuery(
    HousekeepingStatus? Status = null,
    Guid? AssignedToUserId = null,
    int Page = 1,
    int PageSize = 50) : IRequest<GetHousekeepingTasksResult>;

public record GetHousekeepingTasksResult(
    IReadOnlyList<HousekeepingTaskDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
