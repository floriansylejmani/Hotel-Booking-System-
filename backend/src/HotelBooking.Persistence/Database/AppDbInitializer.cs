using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using HotelBooking.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Persistence.Database;

public class AppDbInitializer(AppDbContext db, IPasswordHasher passwordHasher)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        await SeedRolesAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
        await SeedRoomsAsync(cancellationToken);
        await SeedGuestUsersAsync(cancellationToken);
        await SeedHousekeepingStaffAsync(cancellationToken);
        await SeedBookingsAsync(cancellationToken);
        await SeedRoomChargesAsync(cancellationToken);
        await SeedHousekeepingTasksAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var existingRoleNames = await db.Roles
            .AsNoTracking()
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var missingRoles = RoleNames.All
            .Except(existingRoleNames, StringComparer.OrdinalIgnoreCase)
            .Select(roleName => new Role
            {
                Id = GetRoleId(roleName),
                Name = roleName,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (missingRoles.Count == 0)
            return;

        await db.Roles.AddRangeAsync(missingRoles, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@hotel.com";

        var adminExists = await db.Users
            .AnyAsync(user => user.Email == adminEmail, cancellationToken);

        if (adminExists)
            return;

        var adminRole = await db.Roles
            .FirstOrDefaultAsync(role => role.Name == RoleNames.Admin, cancellationToken)
            ?? throw new InvalidOperationException("Admin role is missing from the seed configuration.");

        var adminUser = new User
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            FullName = "System Administrator",
            Email = adminEmail,
            PasswordHash = passwordHasher.Hash("Admin123!"),
            RoleId = adminRole.Id,
            Role = adminRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(adminUser);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRoomsAsync(CancellationToken cancellationToken)
    {
        if (await db.Rooms.AnyAsync(cancellationToken))
            return;

        var rooms = new List<Room>
        {
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                RoomNumber = "101",
                FloorNumber = 1,
                Type = RoomType.Standard,
                Status = RoomStatus.Occupied,
                PricePerNight = 89.00m,
                BedCount = 1,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Fridge"],
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                RoomNumber = "102",
                FloorNumber = 1,
                Type = RoomType.Standard,
                Status = RoomStatus.Occupied,
                PricePerNight = 89.00m,
                BedCount = 2,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Fridge"],
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                RoomNumber = "201",
                FloorNumber = 2,
                Type = RoomType.Deluxe,
                Status = RoomStatus.Occupied,
                PricePerNight = 149.00m,
                BedCount = 1,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Bathtub", "City View"],
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                RoomNumber = "202",
                FloorNumber = 2,
                Type = RoomType.Deluxe,
                Status = RoomStatus.Occupied,
                PricePerNight = 149.00m,
                BedCount = 2,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Bathtub", "City View"],
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                RoomNumber = "301",
                FloorNumber = 3,
                Type = RoomType.Family,
                Status = RoomStatus.Available,
                PricePerNight = 199.00m,
                BedCount = 3,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Sofa Bed", "Kitchenette", "Garden View"],
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
                RoomNumber = "401",
                FloorNumber = 4,
                Type = RoomType.BusinessSuite,
                Status = RoomStatus.Available,
                PricePerNight = 299.00m,
                BedCount = 1,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Jacuzzi", "Living Room", "Panoramic View", "Work Desk", "Espresso Machine"],
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000007"),
                RoomNumber = "402",
                FloorNumber = 4,
                Type = RoomType.BusinessSuite,
                Status = RoomStatus.Maintenance,
                PricePerNight = 299.00m,
                BedCount = 2,
                Amenities = ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Jacuzzi", "Living Room", "Panoramic View", "Work Desk", "Espresso Machine"],
                CreatedAt = DateTime.UtcNow
            }
        };

        await db.Rooms.AddRangeAsync(rooms, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedGuestUsersAsync(CancellationToken cancellationToken)
    {
        var guestRole = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == RoleNames.Guest, cancellationToken)
            ?? throw new InvalidOperationException("Guest role is missing from the seed configuration.");

        var guestUsers = new[]
        {
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), Name = "James Carter",   Email = "james.carter@example.com" },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), Name = "The Williams",   Email = "the.williams@example.com" },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), Name = "Anna Müller",    Email = "anna.muller@example.com" },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000004"), Name = "Sofia Reyes",    Email = "sofia.reyes@example.com" },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000005"), Name = "Liam Chen",      Email = "liam.chen@example.com" }
        };

        var existingEmails = await db.Users
            .AsNoTracking()
            .Where(u => guestUsers.Select(g => g.Email).Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync(cancellationToken);

        var toAdd = guestUsers
            .Where(g => !existingEmails.Contains(g.Email))
            .Select(g => new User
            {
                Id = g.Id,
                FullName = g.Name,
                Email = g.Email,
                PasswordHash = passwordHasher.Hash("Guest123!"),
                RoleId = guestRole.Id,
                Role = guestRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (toAdd.Count == 0)
            return;

        await db.Users.AddRangeAsync(toAdd, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedBookingsAsync(CancellationToken cancellationToken)
    {
        if (await db.Bookings.AnyAsync(cancellationToken))
            return;

        // Room IDs from SeedRoomsAsync
        var room101 = Guid.Parse("20000000-0000-0000-0000-000000000001"); // Standard  $89
        var room102 = Guid.Parse("20000000-0000-0000-0000-000000000002"); // Standard  $89
        var room201 = Guid.Parse("20000000-0000-0000-0000-000000000003"); // Deluxe   $149
        var room202 = Guid.Parse("20000000-0000-0000-0000-000000000004"); // Deluxe   $149
        var room301 = Guid.Parse("20000000-0000-0000-0000-000000000005"); // Family   $199

        // Guest user IDs from SeedGuestUsersAsync
        var jamesCarter  = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var theWilliams  = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var annaMuller   = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var sofiaReyes   = Guid.Parse("30000000-0000-0000-0000-000000000004");
        var liamChen     = Guid.Parse("30000000-0000-0000-0000-000000000005");

        var bookings = new List<Booking>
        {
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                BookingCode = "BK-2401",
                UserId = jamesCarter,
                RoomId = room102,
                CheckInDate = new DateOnly(2026, 4, 18),
                CheckOutDate = new DateOnly(2026, 4, 22),
                TotalAmount = 356.00m,     // 4 nights × $89
                PaymentMethod = PaymentMethod.Card,
                Status = BookingStatus.Active,
                GuestCount = 2,
                CreatedAt = new DateTime(2026, 4, 15, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                BookingCode = "BK-2402",
                UserId = theWilliams,
                RoomId = room201,
                CheckInDate = new DateOnly(2026, 4, 20),
                CheckOutDate = new DateOnly(2026, 4, 25),
                TotalAmount = 745.00m,     // 5 nights × $149
                PaymentMethod = PaymentMethod.Card,
                Status = BookingStatus.Active,
                GuestCount = 3,
                CreatedAt = new DateTime(2026, 4, 16, 9, 30, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                BookingCode = "BK-2403",
                UserId = annaMuller,
                RoomId = room202,
                CheckInDate = new DateOnly(2026, 4, 21),
                CheckOutDate = new DateOnly(2026, 4, 23),
                TotalAmount = 298.00m,     // 2 nights × $149
                PaymentMethod = PaymentMethod.Cash,
                Status = BookingStatus.Active,
                GuestCount = 1,
                CreatedAt = new DateTime(2026, 4, 17, 14, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                BookingCode = "BK-2404",
                UserId = sofiaReyes,
                RoomId = room101,
                CheckInDate = new DateOnly(2026, 4, 22),
                CheckOutDate = new DateOnly(2026, 4, 28),
                TotalAmount = 534.00m,     // 6 nights × $89
                PaymentMethod = PaymentMethod.Card,
                Status = BookingStatus.Confirmed,
                GuestCount = 1,
                CreatedAt = new DateTime(2026, 4, 18, 11, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000005"),
                BookingCode = "BK-2405",
                UserId = liamChen,
                RoomId = room301,
                CheckInDate = new DateOnly(2026, 4, 23),
                CheckOutDate = new DateOnly(2026, 4, 25),
                TotalAmount = 398.00m,     // 2 nights × $199
                PaymentMethod = PaymentMethod.Check,
                Status = BookingStatus.Confirmed,
                GuestCount = 4,
                CreatedAt = new DateTime(2026, 4, 19, 8, 0, 0, DateTimeKind.Utc)
            }
        };

        await db.Bookings.AddRangeAsync(bookings, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRoomChargesAsync(CancellationToken cancellationToken)
    {
        if (await db.RoomCharges.AnyAsync(cancellationToken))
            return;

        var charges = new List<RoomCharge>
        {
            new()
            {
                BookingId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Description = "Room Service",
                Amount = 42.50m
            },
            new()
            {
                BookingId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Description = "Minibar",
                Amount = 18.00m
            },
            new()
            {
                BookingId = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                Description = "Parking",
                Amount = 55.00m
            },
            new()
            {
                BookingId = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                Description = "Late Checkout",
                Amount = 25.00m
            }
        };

        await db.RoomCharges.AddRangeAsync(charges, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedHousekeepingStaffAsync(CancellationToken cancellationToken)
    {
        var housekeeperRole = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == RoleNames.Housekeeper, cancellationToken)
            ?? throw new InvalidOperationException("Housekeeper role is missing from the seed configuration.");

        var staffUsers = new[]
        {
            new { Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Maria Santos",  Email = "maria.santos@hotel.com" },
            new { Id = Guid.Parse("50000000-0000-0000-0000-000000000002"), Name = "Carlos Rivera", Email = "carlos.rivera@hotel.com" }
        };

        var existingEmails = await db.Users
            .AsNoTracking()
            .Where(u => staffUsers.Select(s => s.Email).Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync(cancellationToken);

        var toAdd = staffUsers
            .Where(s => !existingEmails.Contains(s.Email))
            .Select(s => new User
            {
                Id = s.Id,
                FullName = s.Name,
                Email = s.Email,
                PasswordHash = passwordHasher.Hash("Staff123!"),
                RoleId = housekeeperRole.Id,
                Role = housekeeperRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (toAdd.Count == 0)
            return;

        await db.Users.AddRangeAsync(toAdd, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedHousekeepingTasksAsync(CancellationToken cancellationToken)
    {
        if (await db.HousekeepingTasks.AnyAsync(cancellationToken))
            return;

        var mariaSantos  = Guid.Parse("50000000-0000-0000-0000-000000000001");
        var carlosRivera = Guid.Parse("50000000-0000-0000-0000-000000000002");

        // Room IDs mirror SeedRoomsAsync
        var room101 = Guid.Parse("20000000-0000-0000-0000-000000000001"); // Standard,       Occupied
        var room102 = Guid.Parse("20000000-0000-0000-0000-000000000002"); // Standard,       Occupied
        var room201 = Guid.Parse("20000000-0000-0000-0000-000000000003"); // Deluxe,         Occupied
        var room202 = Guid.Parse("20000000-0000-0000-0000-000000000004"); // Deluxe,         Occupied
        var room301 = Guid.Parse("20000000-0000-0000-0000-000000000005"); // Family,         Available
        var room401 = Guid.Parse("20000000-0000-0000-0000-000000000006"); // BusinessSuite,  Available
        var room402 = Guid.Parse("20000000-0000-0000-0000-000000000007"); // BusinessSuite,  Maintenance

        var tasks = new List<HousekeepingTask>
        {
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                RoomId = room101,
                Status = HousekeepingStatus.Dirty,
                AssignedToUserId = mariaSantos,
                Notes = "Guest checked out. Full turnover required.",
                LastCleanedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000002"),
                RoomId = room102,
                Status = HousekeepingStatus.InProgress,
                AssignedToUserId = carlosRivera,
                Notes = "Daily service in progress.",
                LastCleanedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000003"),
                RoomId = room201,
                Status = HousekeepingStatus.Dirty,
                AssignedToUserId = null,
                Notes = "Awaiting assignment.",
                LastCleanedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000004"),
                RoomId = room202,
                Status = HousekeepingStatus.Dirty,
                AssignedToUserId = mariaSantos,
                Notes = "Extra towels requested.",
                LastCleanedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000005"),
                RoomId = room301,
                Status = HousekeepingStatus.Clean,
                AssignedToUserId = mariaSantos,
                Notes = "Ready for next guest.",
                LastCleanedAt = DateTime.UtcNow.AddHours(-2),
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000006"),
                RoomId = room401,
                Status = HousekeepingStatus.Clean,
                AssignedToUserId = carlosRivera,
                Notes = "Deep cleaned and inspected.",
                LastCleanedAt = DateTime.UtcNow.AddHours(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new()
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000007"),
                RoomId = room402,
                Status = HousekeepingStatus.Maintenance,
                AssignedToUserId = null,
                Notes = "Bathroom fixture replacement in progress. Do not assign guests.",
                LastCleanedAt = null,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        await db.HousekeepingTasks.AddRangeAsync(tasks, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Guid GetRoleId(string roleName) => roleName switch
    {
        RoleNames.Admin => Guid.Parse("00000000-0000-0000-0000-000000000001"),
        RoleNames.Manager => Guid.Parse("00000000-0000-0000-0000-000000000002"),
        RoleNames.Receptionist => Guid.Parse("00000000-0000-0000-0000-000000000003"),
        RoleNames.Housekeeper => Guid.Parse("00000000-0000-0000-0000-000000000004"),
        RoleNames.Guest => Guid.Parse("00000000-0000-0000-0000-000000000005"),
        _ => throw new InvalidOperationException($"Unknown role '{roleName}'.")
    };
}
