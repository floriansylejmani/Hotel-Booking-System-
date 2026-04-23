namespace HotelBooking.Domain.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Receptionist = "Receptionist";
    public const string Housekeeper = "Housekeeper";
    public const string Guest = "Guest";

    public static readonly string[] All =
    [
        Admin,
        Manager,
        Receptionist,
        Housekeeper,
        Guest
    ];

    public static string Normalize(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Guest;
        }

        return All.FirstOrDefault(role => role.Equals(roleName.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? roleName.Trim();
    }
}
