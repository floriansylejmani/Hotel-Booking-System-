using FluentValidation;
using HotelBooking.Application.Features.Staff.DTOs;
using HotelBooking.Domain.Constants;

namespace HotelBooking.Application.Features.Staff.Validators;

public sealed class UpdateStaffRequestValidator : AbstractValidator<UpdateStaffRequest>
{
    public UpdateStaffRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name cannot be empty.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.")
            .When(x => x.FullName is not null);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .When(x => x.Email is not null);

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name cannot be empty.")
            .Must(r => RoleNames.All.Contains(RoleNames.Normalize(r!)))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.")
            .When(x => x.RoleName is not null);
    }
}
