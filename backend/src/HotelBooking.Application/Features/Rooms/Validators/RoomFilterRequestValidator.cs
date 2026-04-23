using FluentValidation;
using HotelBooking.Application.Features.Rooms.DTOs;

namespace HotelBooking.Application.Features.Rooms.Validators;

public sealed class RoomFilterRequestValidator : AbstractValidator<RoomFilterRequest>
{
    public RoomFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Floor)
            .GreaterThan(0)
            .When(x => x.Floor.HasValue);

        RuleFor(x => x.Search)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
