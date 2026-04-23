using FluentValidation;

namespace HotelBooking.Application.Features.Housekeeping.Commands.UpdateTaskStatus;

public sealed class UpdateHousekeepingStatusCommandValidator : AbstractValidator<UpdateHousekeepingStatusCommand>
{
    public UpdateHousekeepingStatusCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
