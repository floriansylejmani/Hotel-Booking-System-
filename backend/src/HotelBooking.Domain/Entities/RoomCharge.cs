using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public class RoomCharge : BaseEntity
{
    public Guid BookingId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public Booking Booking { get; set; } = null!;
}
