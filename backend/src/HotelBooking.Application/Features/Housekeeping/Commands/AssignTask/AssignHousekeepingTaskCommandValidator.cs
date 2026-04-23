using FluentValidation;

namespace HotelBooking.Application.Features.Housekeeping.Commands.AssignTask;

public sealed class AssignHousekeepingTaskCommandValidator : AbstractValidator<AssignHousekeepingTaskCommand>
{
    public AssignHousekeepingTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.StaffUserId)
            .NotEmpty();
    }
}
