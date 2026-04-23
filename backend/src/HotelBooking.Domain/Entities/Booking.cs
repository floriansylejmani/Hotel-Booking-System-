using HotelBooking.Domain.Common;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public string BookingCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public PaymentMethod PaymentMethod { get; set; }
    public int GuestCount { get; set; }
    public string? SpecialRequests { get; set; }
    public decimal TotalAmount { get; set; }

    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    public ICollection<RoomCharge> RoomCharges { get; set; } = [];
}
