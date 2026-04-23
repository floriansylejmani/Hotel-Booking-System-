using FluentValidation;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Application.Features.Rooms.Validators;

public sealed class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.RoomNumber)
            .MaximumLength(10).WithMessage("Room number must not exceed 10 characters.")
            .When(x => x.RoomNumber is not null);

        RuleFor(x => x.FloorNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Floor number must be 0 or greater.")
            .When(x => x.FloorNumber.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<RoomType>())}.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Price per night must be greater than 0.")
            .When(x => x.PricePerNight.HasValue);

        RuleFor(x => x.BedCount)
            .GreaterThanOrEqualTo(1).WithMessage("Bed count must be at least 1.")
            .When(x => x.BedCount.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<RoomStatus>())}.")
            .When(x => x.Status.HasValue);
    }
}
