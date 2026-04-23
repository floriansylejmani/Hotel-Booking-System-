using FluentValidation;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Rooms.Validators;

public sealed class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Room number is required.")
            .MaximumLength(10).WithMessage("Room number must not exceed 10 characters.");

        RuleFor(x => x.FloorNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Floor number must be 0 or greater.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<RoomType>())}.");

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Price per night must be greater than 0.");

        RuleFor(x => x.BedCount)
            .GreaterThanOrEqualTo(1).WithMessage("Bed count must be at least 1.");

        RuleFor(x => x.Amenities)
            .NotNull().WithMessage("Amenities must not be null.");
    }
}
