using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
