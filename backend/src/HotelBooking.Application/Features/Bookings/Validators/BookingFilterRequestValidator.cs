using FluentValidation;
using HotelBooking.Application.Features.Bookings.DTOs;

namespace HotelBooking.Application.Features.Bookings.Validators;

public sealed class BookingFilterRequestValidator : AbstractValidator<BookingFilterRequest>
{
    public BookingFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.CheckInTo)
            .GreaterThanOrEqualTo(x => x.CheckInFrom!.Value)
            .When(x => x.CheckInFrom.HasValue && x.CheckInTo.HasValue);

        RuleFor(x => x.BookingCode)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.BookingCode));

        RuleFor(x => x.RoomNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.RoomNumber));

        RuleFor(x => x.GuestName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.GuestName));
    }
}
