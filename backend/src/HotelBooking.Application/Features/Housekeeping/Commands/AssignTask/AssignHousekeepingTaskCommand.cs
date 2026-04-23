using HotelBooking.Application.Features.Housekeeping.DTOs;
using MediatR;

namespace HotelBooking.Application.Features.Housekeeping.Commands.AssignTask;

public record AssignHousekeepingTaskCommand(Guid TaskId, Guid StaffUserId) : IRequest<HousekeepingTaskDto>;
