using HotelBooking.Domain.Common;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Domain.Entities;

public class HousekeepingTask : BaseEntity
{
    public Guid RoomId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public HousekeepingStatus Status { get; set; } = HousekeepingStatus.Dirty;
    public string Notes { get; set; } = string.Empty;
    public DateTime? LastCleanedAt { get; set; }

    public Room Room { get; set; } = null!;
    public User? AssignedTo { get; set; }
}
