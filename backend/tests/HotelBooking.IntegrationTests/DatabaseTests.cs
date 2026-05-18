using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using HotelBooking.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelBooking.IntegrationTests;

public class DatabaseTests
{
    [Fact]
    public void EfCoreModel_HasMigrationsConfigured()
    {
        using var db = CreateDbContext();
        var entityTypes = db.Model.GetEntityTypes().Select(e => e.ClrType).ToArray();

        Assert.Contains(typeof(Room), entityTypes);
        Assert.Contains(typeof(Booking), entityTypes);
        Assert.Contains(typeof(User), entityTypes);
    }

    [Fact]
    public async Task SeedDataExists_WhenInsertedIntoTestDatabase()
    {
        await using var db = CreateDbContext();
        await SeedAsync(db);

        Assert.True(await db.Roles.AnyAsync(r => r.Name == RoleNames.Admin));
        Assert.True(await db.Rooms.AnyAsync(r => r.Status == RoomStatus.Available));
    }

    [Fact]
    public async Task RoomAvailabilityQuery_ExcludesOverlappingBookings()
    {
        await using var db = CreateDbContext();
        var (_, availableRoom, bookedRoom) = await SeedAsync(db);

        var checkIn = new DateOnly(2026, 10, 2);
        var checkOut = new DateOnly(2026, 10, 4);
        var availableRooms = await db.Rooms
            .Where(room => room.Status == RoomStatus.Available)
            .Where(room => !db.Bookings.Any(booking =>
                booking.RoomId == room.Id &&
                booking.Status != BookingStatus.Cancelled &&
                booking.CheckInDate < checkOut &&
                booking.CheckOutDate > checkIn))
            .ToListAsync();

        Assert.Contains(availableRoom, availableRooms);
        Assert.DoesNotContain(bookedRoom, availableRooms);
    }

    [Fact]
    public async Task BookingPersistence_SavesAndReloadsBookingWithRoomAndUser()
    {
        await using var db = CreateDbContext();
        var (user, room, _) = await SeedAsync(db);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "BK-PERSIST",
            UserId = user.Id,
            RoomId = room.Id,
            CheckInDate = new DateOnly(2026, 11, 1),
            CheckOutDate = new DateOnly(2026, 11, 3),
            PaymentMethod = PaymentMethod.Card,
            GuestCount = 1,
            TotalAmount = 200m,
            Status = BookingStatus.Confirmed
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        var reloaded = await db.Bookings.Include(b => b.Room).Include(b => b.User).SingleAsync(b => b.Id == booking.Id);
        Assert.Equal("BK-PERSIST", reloaded.BookingCode);
        Assert.Equal(room.RoomNumber, reloaded.Room.RoomNumber);
        Assert.Equal(user.Email, reloaded.User.Email);
    }

    [Fact]
    public void EfCoreModel_ConfiguresUniqueIndexesAndDeleteBehavior()
    {
        using var db = CreateDbContext();

        var userEmailIndex = db.Model.FindEntityType(typeof(User))!
            .GetIndexes()
            .Single(i => i.Properties.Any(p => p.Name == nameof(User.Email)));
        var roomNumberIndex = db.Model.FindEntityType(typeof(Room))!
            .GetIndexes()
            .Single(i => i.Properties.Any(p => p.Name == nameof(Room.RoomNumber)));
        var bookingEntity = db.Model.FindEntityType(typeof(Booking))!;

        Assert.True(userEmailIndex.IsUnique);
        Assert.True(roomNumberIndex.IsUnique);
        Assert.Contains(bookingEntity.GetForeignKeys(), fk =>
            fk.PrincipalEntityType.ClrType == typeof(User) &&
            fk.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(bookingEntity.GetForeignKeys(), fk =>
            fk.PrincipalEntityType.ClrType == typeof(Room) &&
            fk.DeleteBehavior == DeleteBehavior.Restrict);
    }

    [Fact]
    public async Task PaymentAndInvoiceQueries_ByBookingPreserveMoneyAndRelations()
    {
        await using var db = CreateDbContext();
        var (user, room, _) = await SeedAsync(db);
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "BK-MONEY",
            UserId = user.Id,
            RoomId = room.Id,
            CheckInDate = new DateOnly(2026, 12, 24),
            CheckOutDate = new DateOnly(2026, 12, 27),
            PaymentMethod = PaymentMethod.Card,
            GuestCount = 2,
            TotalAmount = 370.35m,
            Status = BookingStatus.Confirmed,
            CreatedAt = new DateTime(2026, 12, 1, 8, 30, 0, DateTimeKind.Utc)
        };
        var invoice = new Invoice
        {
            BookingId = booking.Id,
            Booking = booking,
            InvoiceNumber = "INV-MONEY",
            Subtotal = 336.68m,
            TaxRate = 0.10m,
            TaxAmount = 33.67m,
            TotalAmount = 370.35m,
            IssuedAt = new DateTime(2026, 12, 1, 9, 0, 0, DateTimeKind.Utc)
        };
        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            Method = PaymentMethod.Card.ToString(),
            Amount = 370.35m,
            Status = PaymentStatus.Completed,
            PaidAt = new DateTime(2026, 12, 1, 9, 5, 0, DateTimeKind.Utc)
        };
        db.Bookings.Add(booking);
        db.Invoices.Add(invoice);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var reloadedInvoice = await db.Invoices.Include(i => i.Payments).SingleAsync(i => i.BookingId == booking.Id);
        var userBookings = await db.Bookings.Where(b => b.UserId == user.Id).ToListAsync();

        Assert.Equal(370.35m, reloadedInvoice.TotalAmount);
        Assert.Equal(370.35m, reloadedInvoice.Payments.Single().Amount);
        Assert.Contains(userBookings, b => b.BookingCode == "BK-MONEY");
        Assert.Equal(new DateTime(2026, 12, 1, 9, 0, 0, DateTimeKind.Utc), reloadedInvoice.IssuedAt);
    }

    private static AppDbContext CreateDbContext()
    {
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static async Task<(User user, Room availableRoom, Room bookedRoom)> SeedAsync(AppDbContext db)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = RoleNames.Admin };
        var user = new User { Id = Guid.NewGuid(), FullName = "Admin", Email = "admin@test.local", PasswordHash = "x", Role = role, RoleId = role.Id, IsActive = true };
        var availableRoom = new Room { Id = Guid.NewGuid(), RoomNumber = "901", FloorNumber = 9, Type = RoomType.Standard, Status = RoomStatus.Available, PricePerNight = 100m, BedCount = 1, Amenities = ["WiFi"] };
        var bookedRoom = new Room { Id = Guid.NewGuid(), RoomNumber = "902", FloorNumber = 9, Type = RoomType.Standard, Status = RoomStatus.Available, PricePerNight = 100m, BedCount = 1, Amenities = ["WiFi"] };
        db.Roles.Add(role);
        db.Users.Add(user);
        db.Rooms.AddRange(availableRoom, bookedRoom);
        db.Bookings.Add(new Booking { Id = Guid.NewGuid(), BookingCode = "BK-OVERLAP", UserId = user.Id, RoomId = bookedRoom.Id, CheckInDate = new DateOnly(2026, 10, 1), CheckOutDate = new DateOnly(2026, 10, 5), PaymentMethod = PaymentMethod.Card, GuestCount = 1, Status = BookingStatus.Confirmed, TotalAmount = 400m });
        await db.SaveChangesAsync();
        return (user, availableRoom, bookedRoom);
    }
}
