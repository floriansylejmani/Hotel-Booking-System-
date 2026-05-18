using FluentValidation;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Settings;
using HotelBooking.Application.Features.Bookings;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Application.Features.Bookings.Validators;
using HotelBooking.Application.Features.CheckInOut;
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

public class AdvancedBusinessRuleTests
{
    [Fact]
    public async Task Booking_BackToBackDates_AreAllowed()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        var service = CreateBookingService(db);

        await service.CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(10), TodayPlus(12), PaymentMethod.Card, 1), user.Id);
        var second = await service.CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(12), TodayPlus(14), PaymentMethod.Card, 1), user.Id);

        Assert.Equal(200m, second.TotalAmount);
    }

    [Fact]
    public async Task Booking_OverlappingDatesSameRoom_AreRejected()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        var service = CreateBookingService(db);

        await service.CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(20), TodayPlus(25), PaymentMethod.Card, 1), user.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(24), TodayPlus(26), PaymentMethod.Card, 1), user.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task Booking_InvalidGuestCounts_AreRejected(int guestCount)
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateBookingService(db).CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(1), TodayPlus(2), PaymentMethod.Card, guestCount), user.Id));
    }

    [Fact]
    public async Task Booking_GuestCountCannotExceedRoomCapacity()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db, bedCount: 2);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            CreateBookingService(db).CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(1), TodayPlus(2), PaymentMethod.Card, 3), user.Id));

        Assert.Contains("capacity", ex.Message);
    }

    [Fact]
    public async Task Booking_ZeroNightsAndPastCheckIn_AreRejected()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        var service = CreateBookingService(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(2), TodayPlus(2), PaymentMethod.Card, 1), user.Id));
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(-1), TodayPlus(2), PaymentMethod.Card, 1), user.Id));
    }

    [Theory]
    [InlineData(RoomStatus.Occupied)]
    [InlineData(RoomStatus.Cleaning)]
    [InlineData(RoomStatus.Maintenance)]
    public async Task Booking_NonAvailableRoom_CannotBeBooked(RoomStatus status)
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        room.Status = status;
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            CreateBookingService(db).CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(1), TodayPlus(2), PaymentMethod.Card, 1), user.Id));
    }

    [Fact]
    public async Task Room_DeleteWithActiveBooking_IsRejected()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        await CreateBookingService(db).CreateAsync(new CreateBookingRequest(null, room.Id, TodayPlus(1), TodayPlus(3), PaymentMethod.Card, 1), user.Id);

        await Assert.ThrowsAsync<ConflictException>(() => CreateRoomService(db).DeleteAsync(room.Id));
    }

    [Fact]
    public async Task CompletedBooking_CannotBeCanceled()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        var booking = new Booking { BookingCode = "BK-DONE", UserId = user.Id, RoomId = room.Id, CheckInDate = TodayPlus(1), CheckOutDate = TodayPlus(2), PaymentMethod = PaymentMethod.Card, GuestCount = 1, TotalAmount = 100m, Status = BookingStatus.CheckedOut };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => CreateBookingService(db).CancelAsync(booking.Id, user.Id, RoleNames.Guest));
    }

    [Fact]
    public async Task CheckIn_BeforeScheduledDate_IsRejected()
    {
        await using var db = CreateDbContext();
        var (user, room) = await SeedUserAndRoomAsync(db);
        var booking = new Booking { BookingCode = "BK-FUTURE", User = user, UserId = user.Id, Room = room, RoomId = room.Id, CheckInDate = TodayPlus(2), CheckOutDate = TodayPlus(3), PaymentMethod = PaymentMethod.Card, GuestCount = 1, TotalAmount = 100m, Status = BookingStatus.Confirmed };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => CreateCheckInOutService(db).CheckInAsync(booking.Id));
    }

    [Fact]
    public async Task Payment_ExactRemainingBalanceRequired()
    {
        await using var db = CreateDbContext();
        var booking = await SeedBookingForPaymentAsync(db);
        var invoiceService = CreateInvoiceService(db);
        var invoice = await invoiceService.GetByBookingIdAsync(booking.Id);
        var service = CreatePaymentService(db, invoiceService);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoice.TotalAmount - 1)));
        var paid = await service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoice.TotalAmount));
        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ProcessAsync(new ProcessPaymentRequest(booking.Id, PaymentMethod.Card.ToString(), invoice.TotalAmount)));

        Assert.Equal(PaymentStatus.Completed.ToString(), paid.Status);
    }

    [Fact]
    public async Task Invoice_CancelledBooking_IsRejected()
    {
        await using var db = CreateDbContext();
        var booking = await SeedBookingForPaymentAsync(db);
        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => CreateInvoiceService(db).GetByBookingIdAsync(booking.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Room_PriceValidation_RejectsNonPositiveValues(decimal price)
    {
        await using var db = CreateDbContext();

        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateRoomService(db).CreateAsync(new CreateRoomRequest("R1", 1, RoomType.Standard, price, 1, [])));
    }

    [Fact]
    public async Task Room_UpdateDuplicateRoomNumber_IsRejected()
    {
        await using var db = CreateDbContext();
        var service = CreateRoomService(db);
        var first = await service.CreateAsync(new CreateRoomRequest("801", 8, RoomType.Standard, 100m, 1, []));
        var second = await service.CreateAsync(new CreateRoomRequest("802", 8, RoomType.Standard, 100m, 1, []));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(second.Id, new UpdateRoomRequest(first.RoomNumber, null, null, null, null, null, null)));
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static DateOnly TodayPlus(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static async Task<(User user, Room room)> SeedUserAndRoomAsync(AppDbContext db, int bedCount = 2)
    {
        var role = new Role { Name = RoleNames.Guest };
        var user = new User { FullName = "Guest", Email = $"{Guid.NewGuid():N}@test.local", PasswordHash = "x", Role = role, RoleId = role.Id, IsActive = true };
        var room = new Room { RoomNumber = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), FloorNumber = 2, Type = RoomType.Standard, Status = RoomStatus.Available, PricePerNight = 100m, BedCount = bedCount, Amenities = ["WiFi"] };
        db.Roles.Add(role);
        db.Users.Add(user);
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        return (user, room);
    }

    private static async Task<Booking> SeedBookingForPaymentAsync(AppDbContext db)
    {
        var (user, room) = await SeedUserAndRoomAsync(db);
        var booking = new Booking { BookingCode = "BK-PAY", User = user, UserId = user.Id, Room = room, RoomId = room.Id, CheckInDate = TodayPlus(5), CheckOutDate = TodayPlus(7), PaymentMethod = PaymentMethod.Card, GuestCount = 1, TotalAmount = 200m, Status = BookingStatus.Confirmed };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking;
    }

    private static BookingService CreateBookingService(AppDbContext db) =>
        new(db, new CreateBookingRequestValidator(), new BookingFilterRequestValidator(), Mock.Of<IPublisher>(), Mock.Of<HotelBooking.Application.Features.Notifications.INotificationService>(), Mock.Of<IAuditService>(), NullLogger<BookingService>.Instance);

    private static RoomService CreateRoomService(AppDbContext db) =>
        new(db, new CreateRoomRequestValidator(), new UpdateRoomRequestValidator(), new RoomFilterRequestValidator(), Mock.Of<IAuditService>());

    private static CheckInOutService CreateCheckInOutService(AppDbContext db) =>
        new(db, Mock.Of<HotelBooking.Application.Features.Notifications.INotificationService>(), Mock.Of<IPublisher>(), Mock.Of<IAuditService>(), NullLogger<CheckInOutService>.Instance);

    private static InvoiceService CreateInvoiceService(AppDbContext db) =>
        new(db, Mock.Of<IInvoicePdfService>(), Options.Create(new TaxSettings { Rate = 0m }));

    private static PaymentService CreatePaymentService(AppDbContext db, IInvoiceService invoiceService) =>
        new(db, invoiceService, new ProcessPaymentRequestValidator(), Mock.Of<HotelBooking.Application.Features.Notifications.INotificationService>(), Mock.Of<IPublisher>(), Mock.Of<IAuditService>(), NullLogger<PaymentService>.Instance);
}
