using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Models;
using HotelBooking.Application.Common.Settings;
using HotelBooking.Application.Features.Auth;
using HotelBooking.Application.Features.Auth.DTOs;
using HotelBooking.Application.Features.Auth.Validators;
using HotelBooking.Application.Features.Bookings;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Application.Features.Bookings.Validators;
using HotelBooking.Application.Features.CheckInOut;
using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardOccupancy;
using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardRecentBookings;
using HotelBooking.Application.Features.Dashboard.Queries.GetDashboardSummary;
using HotelBooking.Application.Features.Housekeeping.Commands.AssignTask;
using HotelBooking.Application.Features.Housekeeping.Commands.UpdateTaskStatus;
using HotelBooking.Application.Features.Notifications;
using HotelBooking.Application.Features.Payments;
using HotelBooking.Application.Features.Payments.DTOs;
using HotelBooking.Application.Features.Payments.Validators;
using HotelBooking.Application.Features.Rooms;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Application.Features.Rooms.Validators;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using HotelBooking.Infrastructure.Services;
using HotelBooking.Infrastructure.Settings;
using HotelBooking.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class ComprehensiveServiceTests
{
    [Fact]
    public async Task AuthService_Register_HashesPasswordAndRejectsDuplicateEmail()
    {
        await using var db = CreateDbContext();
        await SeedRolesAsync(db);
        var service = CreateAuthService(db);

        await service.RegisterAsync(new RegisterRequest("Jane Guest", "jane@example.com", "Password123!", "Guest"));

        var user = await db.Users.SingleAsync(u => u.Email == "jane@example.com");
        Assert.Equal("hashed:Password123!", user.PasswordHash);
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(new RegisterRequest("Jane Guest", " JANE@example.com ", "Password123!", "Guest")));
    }

    [Fact]
    public async Task AuthService_Login_ReturnsUserDataAndRejectsUnknownEmail()
    {
        await using var db = CreateDbContext();
        var role = await AddRoleAsync(db, RoleNames.Guest);
        db.Users.Add(new User { FullName = "Guest", Email = "guest@example.com", PasswordHash = "hashed:Password123!", Role = role, RoleId = role.Id, IsActive = true });
        await db.SaveChangesAsync();
        var service = CreateAuthService(db);

        var result = await service.LoginAsync(new LoginRequest("guest@example.com", "Password123!"));

        Assert.Equal("guest@example.com", result.Email);
        Assert.Equal(RoleNames.Guest, result.Role);
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest("missing@example.com", "Password123!")));
    }

    [Fact]
    public void JwtTokenService_GeneratesExpectedIdentityAndRoleClaims()
    {
        var role = new Role { Id = Guid.NewGuid(), Name = RoleNames.Admin };
        var user = new User { Id = Guid.NewGuid(), FullName = "Admin User", Email = "admin@test.local", Role = role, RoleId = role.Id };
        var service = new JwtTokenService(Options.Create(new JwtSettings
        {
            Key = "UNIT_TEST_SECRET_KEY_12345678901234567890",
            Issuer = "HotelBookingSystem",
            Audience = "HotelBookingSystem",
            ExpirationMinutes = 60
        }));

        var token = service.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);

        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == RoleNames.Admin);
    }

    [Fact]
    public async Task RoomService_UpdateDeleteAndValidationRules_Work()
    {
        await using var db = CreateDbContext();
        var service = CreateRoomService(db);
        var created = await service.CreateAsync(new CreateRoomRequest("501", 5, RoomType.Deluxe, 250m, 2, ["WiFi"]));

        var updated = await service.UpdateAsync(created.Id, new UpdateRoomRequest("502", null, null, 275m, 3, ["WiFi", "Desk"], RoomStatus.Available));
        await service.DeleteAsync(created.Id);

        Assert.Equal("502", updated.RoomNumber);
        Assert.Equal(275m, updated.PricePerNight);
        Assert.False(await db.Rooms.AnyAsync(r => r.Id == created.Id));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateRoomRequest("BAD", 1, RoomType.Standard, 0m, 1, [])));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateRoomRequest("BAD2", 1, RoomType.Standard, 10m, 0, [])));
    }

    [Fact]
    public async Task RoomService_GetAll_FiltersByTypeStatusFloorAndSearch()
    {
        await using var db = CreateDbContext();
        db.Rooms.AddRange(
            new Room { RoomNumber = "601", FloorNumber = 6, Type = RoomType.Standard, Status = RoomStatus.Available, PricePerNight = 100m, BedCount = 1, Amenities = [] },
            new Room { RoomNumber = "701", FloorNumber = 7, Type = RoomType.Deluxe, Status = RoomStatus.Maintenance, PricePerNight = 200m, BedCount = 2, Amenities = [] });
        await db.SaveChangesAsync();
        var service = CreateRoomService(db);

        var result = await service.GetAllAsync(new RoomFilterRequest(RoomType.Standard, RoomStatus.Available, 6, "60", 1, 20));

        Assert.Single(result.Items);
        Assert.Equal("601", result.Items[0].RoomNumber);
    }

    [Fact]
    public async Task BookingService_CancelRulesAndDateValidation_Work()
    {
        await using var db = CreateDbContext();
        var (guest, otherGuest, room) = await SeedUsersAndRoomAsync(db);
        var service = CreateBookingService(db);
        var request = new CreateBookingRequest(null, room.Id, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 4), PaymentMethod.Card, 2);

        var booking = await service.CreateAsync(request, guest.Id);
        var cancelled = await service.CancelAsync(booking.Id, guest.Id, RoleNames.Guest);

        Assert.Equal(300m, booking.TotalAmount);
        Assert.Equal(BookingStatus.Cancelled.ToString(), cancelled.Status);
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(request with { CheckOutDate = request.CheckInDate.AddDays(-1) }, guest.Id));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.CancelAsync(booking.Id, otherGuest.Id, RoleNames.Guest));
    }

    [Fact]
    public async Task CheckInOutService_EnforcesStatusTransitionsAndUpdatesRoom()
    {
        await using var db = CreateDbContext();
        var (guest, _, room) = await SeedUsersAndRoomAsync(db);
        var booking = new Booking { BookingCode = "BK-CHECK", User = guest, UserId = guest.Id, Room = room, RoomId = room.Id, CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow), CheckOutDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), PaymentMethod = PaymentMethod.Card, GuestCount = 1, TotalAmount = 100m, Status = BookingStatus.Confirmed };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        var service = CreateCheckInOutService(db);

        var checkedIn = await service.CheckInAsync(booking.Id);
        var checkedOut = await service.CheckOutAsync(booking.Id);

        Assert.Equal(BookingStatus.Active.ToString(), checkedIn.BookingStatus);
        Assert.Equal(BookingStatus.CheckedOut.ToString(), checkedOut.BookingStatus);
        Assert.Equal(RoomStatus.Cleaning, room.Status);
        await Assert.ThrowsAsync<NotFoundException>(() => service.CheckOutAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task PaymentService_FullPaymentCompletesAndDuplicatePaymentIsRejectedByRemainingBalance()
    {
        await using var db = CreateDbContext();
        var booking = await SeedBookingWithChargesAsync(db);
        var invoiceService = CreateInvoiceService(db);
        var invoice = await invoiceService.GetByBookingIdAsync(booking.Id);
        var service = CreatePaymentService(db, invoiceService);

        var payment = await service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoice.TotalAmount));

        Assert.Equal(PaymentStatus.Completed.ToString(), payment.Status);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), 1m)));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ProcessAsync(new ProcessPaymentRequest(booking.Id, "Crypto", 1m)));
    }

    [Fact]
    public async Task InvoiceService_IsIdempotentAndGeneratesPdfFromInvoiceData()
    {
        await using var db = CreateDbContext();
        var booking = await SeedBookingWithChargesAsync(db);
        var pdf = new Mock<IInvoicePdfService>();
        pdf.Setup(x => x.GeneratePdf(It.IsAny<InvoiceResponse>())).Returns([4, 5, 6]);
        var service = new InvoiceService(db, pdf.Object, Options.Create(new TaxSettings { Rate = 0.10m }));

        var first = await service.GetByBookingIdAsync(booking.Id);
        var second = await service.GetByBookingIdAsync(booking.Id);
        var bytes = await service.GetPdfByBookingIdAsync(booking.Id);

        Assert.Equal(first.InvoiceId, second.InvoiceId);
        Assert.Equal(first.TotalAmount, second.TotalAmount);
        Assert.Equal([4, 5, 6], bytes);
        pdf.Verify(x => x.GeneratePdf(It.Is<InvoiceResponse>(i => i.InvoiceId == first.InvoiceId)), Times.Once);
    }

    [Fact]
    public async Task NotificationService_CreatesFiltersAndMarksRead()
    {
        await using var db = CreateDbContext();
        var (guest, otherGuest, _) = await SeedUsersAndRoomAsync(db);
        var service = new NotificationService(db);

        await service.CreateAsync(guest.Id, " Booking ", " Confirmed ", NotificationType.BookingCreated);
        await service.CreateAsync(otherGuest.Id, "Other", "Hidden", NotificationType.BookingCancelled);
        var result = await service.GetForUserAsync(guest.Id, unreadOnly: true, pageNumber: 1, pageSize: 20);
        await service.MarkAsReadAsync(result.Items[0].Id, guest.Id);

        var afterRead = await service.GetForUserAsync(guest.Id, unreadOnly: false, pageNumber: 1, pageSize: 20);
        Assert.Single(result.Items);
        Assert.Equal("Booking", result.Items[0].Title);
        Assert.True(afterRead.Items[0].IsRead);
        await Assert.ThrowsAsync<ForbiddenException>(() => service.MarkAsReadAsync(result.Items[0].Id, otherGuest.Id));
    }

    [Fact]
    public async Task DashboardHandlers_CalculateRevenueOccupancyAndRecentBookings()
    {
        await using var db = CreateDbContext();
        var booking = await SeedPaidDashboardDataAsync(db);
        var summary = await new GetDashboardSummaryQueryHandler(db).Handle(new GetDashboardSummaryQuery(booking.UserId), default);
        var occupancy = await new GetDashboardOccupancyQueryHandler(db).Handle(new GetDashboardOccupancyQuery(), default);
        var recent = await new GetDashboardRecentBookingsQueryHandler(db).Handle(new GetDashboardRecentBookingsQuery(5), default);

        Assert.Equal(1, summary.OccupiedRooms);
        Assert.Equal(booking.TotalAmount, summary.MonthlyRevenue);
        Assert.Equal(50m, occupancy.OccupancyPercentage);
        Assert.Contains(recent, b => b.Id == booking.Id);
    }

    [Fact]
    public async Task HousekeepingCommands_AssignAndCompleteTaskUpdateRoomStatus()
    {
        await using var db = CreateDbContext();
        var (guest, _, room) = await SeedUsersAndRoomAsync(db);
        var housekeeperRole = await AddRoleAsync(db, RoleNames.Housekeeper);
        var housekeeper = new User { FullName = "Cleaner", Email = "cleaner@test.local", PasswordHash = "x", Role = housekeeperRole, RoleId = housekeeperRole.Id, IsActive = true };
        var task = new HousekeepingTask { Room = room, RoomId = room.Id, Status = HousekeepingStatus.Dirty, Notes = "Turnover" };
        db.Users.Add(housekeeper);
        db.HousekeepingTasks.Add(task);
        await db.SaveChangesAsync();
        var audit = Mock.Of<IAuditService>();

        var assigned = await new AssignHousekeepingTaskCommandHandler(db, audit).Handle(new AssignHousekeepingTaskCommand(task.Id, housekeeper.Id), default);
        var completed = await new UpdateHousekeepingStatusCommandHandler(db, audit).Handle(new UpdateHousekeepingStatusCommand(task.Id, HousekeepingStatus.Clean, "Done"), default);

        Assert.Equal(housekeeper.FullName, assigned.AssignedTo);
        Assert.Equal(HousekeepingStatus.Clean.ToString(), completed.Status);
        Assert.Equal(RoomStatus.Available, room.Status);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new AssignHousekeepingTaskCommandHandler(db, audit).Handle(new AssignHousekeepingTaskCommand(task.Id, guest.Id), default));
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static AuthService CreateAuthService(AppDbContext db)
    {
        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(new JwtToken("token", DateTime.UtcNow.AddHours(1)));
        return new AuthService(db, new TestPasswordHasher(), jwt.Object, new RegisterRequestValidator(), new LoginRequestValidator(), NullLogger<AuthService>.Instance);
    }

    private static RoomService CreateRoomService(AppDbContext db) =>
        new(db, new CreateRoomRequestValidator(), new UpdateRoomRequestValidator(), new RoomFilterRequestValidator(), Mock.Of<IAuditService>());

    private static BookingService CreateBookingService(AppDbContext db) =>
        new(db, new CreateBookingRequestValidator(), new BookingFilterRequestValidator(), Mock.Of<IPublisher>(), Mock.Of<INotificationService>(), Mock.Of<IAuditService>(), NullLogger<BookingService>.Instance);

    private static CheckInOutService CreateCheckInOutService(AppDbContext db) =>
        new(db, Mock.Of<INotificationService>(), Mock.Of<IPublisher>(), Mock.Of<IAuditService>(), NullLogger<CheckInOutService>.Instance);

    private static InvoiceService CreateInvoiceService(AppDbContext db) =>
        new(db, Mock.Of<IInvoicePdfService>(), Options.Create(new TaxSettings { Rate = 0.10m }));

    private static PaymentService CreatePaymentService(AppDbContext db, IInvoiceService invoiceService) =>
        new(db, invoiceService, new ProcessPaymentRequestValidator(), Mock.Of<INotificationService>(), Mock.Of<IPublisher>(), Mock.Of<IAuditService>(), NullLogger<PaymentService>.Instance);

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        foreach (var role in RoleNames.All)
            await AddRoleAsync(db, role);
    }

    private static async Task<Role> AddRoleAsync(AppDbContext db, string name)
    {
        var existing = await db.Roles.FirstOrDefaultAsync(r => r.Name == name);
        if (existing is not null)
            return existing;

        var role = new Role { Id = Guid.NewGuid(), Name = name };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    private static async Task<(User guest, User otherGuest, Room room)> SeedUsersAndRoomAsync(AppDbContext db)
    {
        var role = await AddRoleAsync(db, RoleNames.Guest);
        var guest = new User { FullName = "Guest One", Email = "guest1@test.local", PasswordHash = "x", Role = role, RoleId = role.Id, IsActive = true };
        var otherGuest = new User { FullName = "Guest Two", Email = "guest2@test.local", PasswordHash = "x", Role = role, RoleId = role.Id, IsActive = true };
        var room = new Room { RoomNumber = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), FloorNumber = 8, Type = RoomType.Standard, Status = RoomStatus.Available, PricePerNight = 100m, BedCount = 2, Amenities = ["WiFi"] };
        db.Users.AddRange(guest, otherGuest);
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        return (guest, otherGuest, room);
    }

    private static async Task<Booking> SeedBookingWithChargesAsync(AppDbContext db)
    {
        var (guest, _, room) = await SeedUsersAndRoomAsync(db);
        var booking = new Booking { BookingCode = "BK-INV", User = guest, UserId = guest.Id, Room = room, RoomId = room.Id, CheckInDate = new DateOnly(2026, 12, 10), CheckOutDate = new DateOnly(2026, 12, 12), PaymentMethod = PaymentMethod.Card, GuestCount = 1, TotalAmount = 200m, Status = BookingStatus.Confirmed };
        booking.RoomCharges.Add(new RoomCharge { BookingId = booking.Id, Description = "Minibar", Amount = 20m });
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking;
    }

    private static async Task<Booking> SeedPaidDashboardDataAsync(AppDbContext db)
    {
        var (guest, _, occupiedRoom) = await SeedUsersAndRoomAsync(db);
        occupiedRoom.Status = RoomStatus.Occupied;
        db.Rooms.Add(new Room { RoomNumber = "FREE", FloorNumber = 9, Type = RoomType.Deluxe, Status = RoomStatus.Available, PricePerNight = 150m, BedCount = 2, Amenities = [] });
        var booking = new Booking { BookingCode = "BK-DASH", User = guest, UserId = guest.Id, Room = occupiedRoom, RoomId = occupiedRoom.Id, CheckInDate = new DateOnly(2026, 12, 1), CheckOutDate = new DateOnly(2026, 12, 2), PaymentMethod = PaymentMethod.Card, GuestCount = 1, TotalAmount = 110m, Status = BookingStatus.Active };
        var invoice = new Invoice { Booking = booking, BookingId = booking.Id, InvoiceNumber = "INV-DASH", Subtotal = 100m, TaxRate = 0.1m, TaxAmount = 10m, TotalAmount = 110m };
        db.Bookings.Add(booking);
        db.Invoices.Add(invoice);
        db.Payments.Add(new Payment { Invoice = invoice, InvoiceId = invoice.Id, Method = PaymentMethod.Card.ToString(), Amount = 110m, Status = PaymentStatus.Completed, PaidAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return booking;
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}
