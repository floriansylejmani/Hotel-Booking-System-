using FluentValidation;
using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Bookings.DTOs;
using HotelBooking.Application.Features.Notifications;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Bookings;

public sealed class BookingService(
    IAppDbContext db,
    IValidator<CreateBookingRequest> createValidator,
    IValidator<BookingFilterRequest> filterValidator,
    IPublisher publisher,
    INotificationService notificationService,
    IAuditService auditService,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        Guid callerId,
        CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = request.UserId ?? callerId;

        var room = await db.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException("Room", request.RoomId);

        if (room.Status == RoomStatus.Maintenance)
            throw new BadRequestException("This room is currently under maintenance and cannot be booked.");

        var overlaps = await db.Bookings.AnyAsync(b =>
            b.RoomId == request.RoomId &&
            b.Status != BookingStatus.Cancelled &&
            b.CheckInDate < request.CheckOutDate &&
            b.CheckOutDate > request.CheckInDate,
            cancellationToken);

        if (overlaps)
            throw new ConflictException("This room is not available for the selected dates.");

        var bookingCode = await GenerateBookingCodeAsync(cancellationToken);
        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        var totalAmount = room.PricePerNight * nights;

        var booking = new Booking
        {
            BookingCode = bookingCode,
            UserId = userId,
            RoomId = room.Id,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            PaymentMethod = request.PaymentMethod,
            GuestCount = request.GuestCount,
            SpecialRequests = request.SpecialRequests,
            TotalAmount = totalAmount,
            Status = BookingStatus.Confirmed
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Booking", booking.Id, "Created", userId,
            $"Room={room.RoomNumber} CheckIn={booking.CheckInDate} CheckOut={booking.CheckOutDate} Total={booking.TotalAmount:C}",
            cancellationToken);

        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        await publisher.Publish(new BookingCreatedEvent(
            booking.Id, userId,
            user.Email, user.FullName,
            room.RoomNumber, booking.CheckInDate, booking.CheckOutDate, totalAmount),
            cancellationToken);

        await notificationService.CreateAsync(
            userId,
            "Booking Confirmed",
            $"Your booking {booking.BookingCode} has been confirmed.",
            NotificationType.BookingCreated,
            cancellationToken);

        logger.LogInformation(
            "Booking created. BookingId={BookingId} BookingCode={BookingCode} UserId={UserId} RoomId={RoomId}",
            booking.Id,
            booking.BookingCode,
            booking.UserId,
            booking.RoomId);

        return ToResponse(booking, user, room, nights);
    }

    public async Task<BookingsPagedResult> GetAllAsync(
        BookingFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return await QueryAsync(filter, cancellationToken);
    }

    public async Task<BookingDetailsResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var b = await db.Bookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Booking", id);

        var nights = b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber;

        return new BookingDetailsResponse(
            b.Id, b.BookingCode, b.UserId,
            b.User.FullName, b.User.Email,
            b.RoomId, b.Room.RoomNumber, b.Room.Type.ToString(),
            b.CheckInDate, b.CheckOutDate,
            b.TotalAmount, b.PaymentMethod.ToString(), b.Status.ToString(),
            nights, b.GuestCount, b.SpecialRequests,
            b.ActualCheckIn, b.ActualCheckOut,
            b.CreatedAt);
    }

    public async Task<BookingResponse> CancelAsync(
        Guid id,
        Guid callerId,
        string callerRole,
        CancellationToken cancellationToken = default)
    {
        var booking = await db.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Booking", id);

        var isStaff = callerRole is RoleNames.Admin or RoleNames.Manager or RoleNames.Receptionist;

        if (booking.UserId != callerId && !isStaff)
            throw new ForbiddenException("You can only cancel your own bookings.");

        if (booking.Status == BookingStatus.CheckedOut)
            throw new BadRequestException("Cannot cancel a booking that has already been checked out.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BadRequestException("This booking is already cancelled.");

        booking.Status = BookingStatus.Cancelled;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Booking", booking.Id, "Cancelled", callerId,
            $"BookingCode={booking.BookingCode} CallerRole={callerRole}",
            cancellationToken);

        await publisher.Publish(new BookingCancelledEvent(
            booking.Id,
            booking.User.Email,
            booking.User.FullName,
            booking.Room.RoomNumber,
            booking.CheckInDate),
            cancellationToken);

        await notificationService.CreateAsync(
            booking.UserId,
            "Booking Cancelled",
            $"Your booking {booking.BookingCode} has been cancelled.",
            NotificationType.BookingCancelled,
            cancellationToken);

        logger.LogInformation(
            "Booking cancelled. BookingId={BookingId} BookingCode={BookingCode} UserId={UserId} CancelledBy={CallerId} CallerRole={CallerRole}",
            booking.Id,
            booking.BookingCode,
            booking.UserId,
            callerId,
            callerRole);

        var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
        return ToResponse(booking, booking.User, booking.Room, nights);
    }

    private async Task<BookingsPagedResult> QueryAsync(
        BookingFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var query = db.Bookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Room)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.GuestName))
        {
            var term = filter.GuestName.Trim().ToLower();
            query = query.Where(b => b.User.FullName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.BookingCode))
            query = query.Where(b => b.BookingCode == filter.BookingCode.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(filter.RoomNumber))
            query = query.Where(b => b.Room.RoomNumber == filter.RoomNumber.Trim().ToUpper());

        if (filter.CheckInFrom.HasValue)
            query = query.Where(b => b.CheckInDate >= filter.CheckInFrom.Value);

        if (filter.CheckInTo.HasValue)
            query = query.Where(b => b.CheckInDate <= filter.CheckInTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => new BookingResponse(
                b.Id, b.BookingCode, b.UserId,
                b.User.FullName, b.User.Email,
                b.RoomId, b.Room.RoomNumber, b.Room.Type.ToString(),
                b.CheckInDate, b.CheckOutDate,
                b.TotalAmount, b.PaymentMethod.ToString(), b.Status.ToString(),
                b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber,
                b.CreatedAt))
            .ToListAsync(cancellationToken);

        return new BookingsPagedResult(items, totalCount, filter.Page, filter.PageSize);
    }

    private async Task<string> GenerateBookingCodeAsync(CancellationToken cancellationToken)
    {
        var lastCode = await db.Bookings
            .OrderByDescending(b => b.BookingCode)
            .Select(b => b.BookingCode)
            .FirstOrDefaultAsync(cancellationToken);

        int next = 1;
        if (lastCode is not null && lastCode.StartsWith("BK-")
            && int.TryParse(lastCode[3..], out var n))
            next = n + 1;

        return $"BK-{next:D4}";
    }

    private static BookingResponse ToResponse(Booking b, User user, Room room, int nights) => new(
        b.Id, b.BookingCode, b.UserId,
        user.FullName, user.Email,
        b.RoomId, room.RoomNumber, room.Type.ToString(),
        b.CheckInDate, b.CheckOutDate,
        b.TotalAmount, b.PaymentMethod.ToString(), b.Status.ToString(),
        nights, b.CreatedAt);
}
