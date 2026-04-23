using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    public Role Role { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<HousekeepingTask> HousekeepingTasks { get; set; } = [];
}
