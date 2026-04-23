using FluentValidation;
using HotelBooking.Application.Features.Staff.DTOs;

namespace HotelBooking.Application.Features.Staff.Validators;

public sealed class StaffFilterRequestValidator : AbstractValidator<StaffFilterRequest>
{
    public StaffFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
