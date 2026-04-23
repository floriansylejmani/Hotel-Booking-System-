using HotelBooking.Domain.Common;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Domain.Entities;

public class Room : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public RoomType Type { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public decimal PricePerNight { get; set; }
    public int BedCount { get; set; }
    public string[] Amenities { get; set; } = [];

    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<HousekeepingTask> HousekeepingTasks { get; set; } = [];
}
