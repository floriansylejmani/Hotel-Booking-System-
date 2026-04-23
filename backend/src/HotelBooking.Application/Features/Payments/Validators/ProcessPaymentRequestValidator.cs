using FluentValidation;
using HotelBooking.Application.Features.Payments.DTOs;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Payments.Validators;

public sealed class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
{
    private static readonly string[] AllowedMethods = Enum
        .GetNames<PaymentMethod>();

    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty();

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .Must(m => AllowedMethods.Contains(m))
            .WithMessage($"PaymentMethod must be one of: {string.Join(", ", AllowedMethods)}.");

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
