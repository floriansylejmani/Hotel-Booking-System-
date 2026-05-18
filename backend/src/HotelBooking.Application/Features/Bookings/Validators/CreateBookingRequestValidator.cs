using FluentValidation;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Bookings.Validators;

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required.");

        RuleFor(x => x.CheckInDate)
            .NotEmpty().WithMessage("Check-in date is required.")
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty().WithMessage("Check-out date is required.")
            .GreaterThan(x => x.CheckInDate).WithMessage("Check-out date must be after check-in date.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage($"Payment method must be one of: {string.Join(", ", Enum.GetNames<PaymentMethod>())}.");

        RuleFor(x => x.GuestCount)
            .InclusiveBetween(1, 20).WithMessage("Guest count must be between 1 and 20.");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(500)
            .Must(value => value is null || !value.Contains("<script", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Special requests contain unsupported markup.");
    }
}
