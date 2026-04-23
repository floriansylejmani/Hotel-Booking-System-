using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Domain.Enums;
using MediatR;

namespace HotelBooking.Application.Features.Housekeeping.Commands.UpdateTaskStatus;

public record UpdateHousekeepingStatusCommand(
    Guid TaskId,
    HousekeepingStatus Status,
    string? Notes = null) : IRequest<HousekeepingTaskDto>;
