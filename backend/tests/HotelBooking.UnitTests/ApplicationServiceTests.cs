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
using HotelBooking.Application.Features.Housekeeping;
using HotelBooking.Application.Features.Housekeeping.DTOs;
using HotelBooking.Application.Features.Housekeeping.Queries.GetTasks;
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
using HotelBooking.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class ApplicationServiceTests
{
    [Fact]
    public async Task AuthService_Register_CreatesGuestAndNormalizesEmail()
    {
        await using var db = CreateDbContext();
        await SeedRolesAsync(db);
        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(new JwtToken("token", DateTime.UtcNow.AddHours(1)));
        var service = new AuthService(db, new TestPasswordHasher(), jwt.Object, new RegisterRequestValidator(), new LoginRequestValidator(), NullLogger<AuthService>.Instance);

        var result = await service.RegisterAsync(new RegisterRequest("Jane Guest", "  JANE@EXAMPLE.COM ", "Password123!", "Guest"));

        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal(RoleNames.Guest, result.Role);
        Assert.True(await db.Users.AnyAsync(u => u.Email == "jane@example.com"));
    }

    [Fact]
    public async Task AuthService_Login_RejectsInvalidPassword()
    {
        await using var db = CreateDbContext();
        var role = await AddRoleAsync(db, RoleNames.Guest);
        db.Users.Add(new User { FullName = "Jane", Email = "jane@example.com", PasswordHash = "hashed:correct", RoleId = role.Id, Role = role, IsActive = true });
        await db.SaveChangesAsync();
        var service = new AuthService(db, new TestPasswordHasher(), Mock.Of<IJwtTokenService>(), new RegisterRequestValidator(), new LoginRequestValidator(), NullLogger<AuthService>.Instance);

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.LoginAsync(new LoginRequest("jane@example.com", "wrong")));
    }

    [Fact]
    public async Task RoomService_Create_NormalizesRoomNumberAndRejectsDuplicates()
    {
        await using var db = CreateDbContext();
        var audit = new Mock<IAuditService>();
        var service = new RoomService(db, new CreateRoomRequestValidator(), new UpdateRoomRequestValidator(), new RoomFilterRequestValidator(), audit.Object);

        var created = await service.CreateAsync(new CreateRoomRequest(" a101 ", 1, RoomType.Standard, 120m, 2, ["WiFi"]));

        Assert.Equal("A101", created.RoomNumber);
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateRoomRequest("A101", 1, RoomType.Standard, 120m, 2, ["WiFi"])));
    }

    [Fact]
    public async Task BookingService_Create_CalculatesTotalAndRejectsDoubleBooking()
    {
        await using var db = CreateDbContext();
        var role = await AddRoleAsync(db, RoleNames.Guest);
        var user = new User { Id = Guid.NewGuid(), FullName = "Guest", Email = "guest@example.com", PasswordHash = "x", RoleId = role.Id, Role = role, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "201", FloorNumber = 2, Type = RoomType.Deluxe, Status = RoomStatus.Available, PricePerNight = 150m, BedCount = 2, Amenities = ["WiFi"] };
        db.Users.Add(user); db.Rooms.Add(room); await db.SaveChangesAsync();
        var service = CreateBookingService(db);
        var request = new CreateBookingRequest(null, room.Id, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 4), PaymentMethod.Card, 2);

        var booking = await service.CreateAsync(request, user.Id);

        Assert.Equal(450m, booking.TotalAmount);
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(request, user.Id));
    }

    [Fact]
    public async Task PaymentService_Process_CreatesPaymentAndRejectsOverpayment()
    {
        await using var db = CreateDbContext();
        var booking = await SeedPaidFlowGraphAsync(db);
        var invoiceService = new InvoiceService(db, Mock.Of<IInvoicePdfService>(), Options.Create(new TaxSettings { Rate = 0.10m }));
        var service = new PaymentService(db, invoiceService, new ProcessPaymentRequestValidator(), Mock.Of<INotificationService>(), Mock.Of<IPublisher>(), Mock.Of<IAuditService>(), NullLogger<PaymentService>.Instance);

        var invoice = await invoiceService.GetByBookingIdAsync(booking.Id);
        var payment = await service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoice.TotalAmount));

        Assert.Equal(PaymentStatus.Completed.ToString(), payment.Status);
        await Assert.ThrowsAsync<BadRequestException>(() => service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoice.TotalAmount)));
    }

    [Fact]
    public async Task InvoiceService_GetByBookingId_IncludesRoomChargesAndGeneratesPdf()
    {
        await using var db = CreateDbContext();
        var booking = await SeedPaidFlowGraphAsync(db);
        var pdf = new Mock<IInvoicePdfService>();
        pdf.Setup(x => x.GeneratePdf(It.IsAny<InvoiceResponse>())).Returns([1, 2, 3]);
        var service = new InvoiceService(db, pdf.Object, Options.Create(new TaxSettings { Rate = 0.10m }));

        var invoice = await service.GetByBookingIdAsync(booking.Id);
        var bytes = await service.GetPdfByBookingIdAsync(booking.Id);

        Assert.Equal(330m, invoice.Subtotal);
        Assert.Equal(363m, invoice.TotalAmount);
        Assert.Equal([1, 2, 3], bytes);
    }

    [Fact]
    public async Task HousekeepingService_DelegatesQueriesAndCommandsToMediator()
    {
        var mediator = new Mock<ISender>();
        var task = new HousekeepingTaskDto(Guid.NewGuid(), "101", HousekeepingStatus.Clean.ToString(), null, "Ready", DateTime.UtcNow);
        mediator.Setup(x => x.Send(It.IsAny<GetHousekeepingTasksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetHousekeepingTasksResult([task], 1, 1, 50));
        var service = new HousekeepingService(mediator.Object);

        var result = await service.GetTasksAsync();

        Assert.Single(result);
        mediator.Verify(x => x.Send(It.IsAny<GetHousekeepingTasksQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AppDbContext CreateDbContext()
    {
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        foreach (var role in RoleNames.All)
            await AddRoleAsync(db, role);
    }

    private static async Task<Role> AddRoleAsync(AppDbContext db, string name)
    {
        var role = new Role { Id = Guid.NewGuid(), Name = name };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    private static BookingService CreateBookingService(AppDbContext db)
    {
        return new BookingService(db, new CreateBookingRequestValidator(), new BookingFilterRequestValidator(), Mock.Of<IPublisher>(), Mock.Of<INotificationService>(), Mock.Of<IAuditService>(), NullLogger<BookingService>.Instance);
    }

    private static async Task<Booking> SeedPaidFlowGraphAsync(AppDbContext db)
    {
        var role = await AddRoleAsync(db, RoleNames.Guest);
        var user = new User { Id = Guid.NewGuid(), FullName = "Guest", Email = "guest@example.com", PasswordHash = "x", RoleId = role.Id, Role = role, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "301", FloorNumber = 3, Type = RoomType.Family, Status = RoomStatus.Available, PricePerNight = 100m, BedCount = 3, Amenities = ["WiFi"] };
        var booking = new Booking { Id = Guid.NewGuid(), BookingCode = "BK-TEST", UserId = user.Id, User = user, RoomId = room.Id, Room = room, CheckInDate = new DateOnly(2026, 8, 1), CheckOutDate = new DateOnly(2026, 8, 4), GuestCount = 2, PaymentMethod = PaymentMethod.Card, Status = BookingStatus.Confirmed, TotalAmount = 300m };
        booking.RoomCharges.Add(new RoomCharge { BookingId = booking.Id, Description = "Minibar", Amount = 30m });
        db.Users.Add(user); db.Rooms.Add(room); db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking;
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}
