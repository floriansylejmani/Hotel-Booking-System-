using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.CheckInOut.DTOs;
using HotelBooking.Application.Features.Notifications;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.CheckInOut;

public sealed class CheckInOutService(
    IAppDbContext db,
    INotificationService notificationService,
    IPublisher publisher,
    IAuditService auditService,
    ILogger<CheckInOutService> logger) : ICheckInOutService
{
    public async Task<CheckInOutResponse> CheckInAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.Status == BookingStatus.Cancelled)
            throw new BadRequestException("Cannot check in: booking is cancelled.");

        if (booking.Status == BookingStatus.Active)
            throw new BadRequestException("Cannot check in: guest is already checked in.");

        if (booking.Status == BookingStatus.CheckedOut)
            throw new BadRequestException("Cannot check in: booking has already been checked out.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BadRequestException($"Cannot check in: booking status is '{booking.Status}'.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (booking.CheckInDate > today)
            throw new BadRequestException(
                $"Cannot check in before the scheduled date ({booking.CheckInDate:yyyy-MM-dd}).");

        if (booking.Room.Status == RoomStatus.Occupied)
            throw new BadRequestException("Cannot check in: the room is currently occupied.");

        if (booking.Room.Status == RoomStatus.Cleaning)
            throw new BadRequestException("Cannot check in: the room is currently being cleaned.");

        if (booking.Room.Status == RoomStatus.Maintenance)
            throw new BadRequestException("Cannot check in: the room is under maintenance.");

        booking.Status = BookingStatus.Active;
        booking.ActualCheckIn = DateTime.UtcNow;
        booking.Room.Status = RoomStatus.Occupied;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Booking", booking.Id, "CheckedIn", booking.UserId,
            $"BookingCode={booking.BookingCode} Room={booking.Room.RoomNumber}",
            cancellationToken);

        await notificationService.CreateAsync(
            booking.UserId,
            "Check-In Completed",
            $"Check-in completed for booking {booking.BookingCode}.",
            NotificationType.CheckInCompleted,
            cancellationToken);

        await publisher.Publish(new CheckInCompletedEvent(
            booking.Id,
            booking.UserId,
            booking.User.Email,
            booking.User.FullName,
            booking.BookingCode,
            booking.Room.RoomNumber,
            booking.ActualCheckIn.Value), cancellationToken);

        logger.LogInformation(
            "Check-in completed. BookingId={BookingId} BookingCode={BookingCode} RoomId={RoomId} UserId={UserId}",
            booking.Id,
            booking.BookingCode,
            booking.RoomId,
            booking.UserId);

        return new CheckInOutResponse(
            booking.Id,
            booking.BookingCode,
            booking.Status.ToString(),
            booking.Room.RoomNumber,
            booking.Room.Status.ToString(),
            booking.ActualCheckIn.Value,
            $"Guest checked in to room {booking.Room.RoomNumber} successfully.");
    }

    public async Task<CheckInOutResponse> CheckOutAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.Status != BookingStatus.Active)
            throw new BadRequestException(
                $"Cannot check out: booking status is '{booking.Status}'. Only Active bookings can be checked out.");

        booking.Status = BookingStatus.CheckedOut;
        booking.ActualCheckOut = DateTime.UtcNow;
        booking.Room.Status = RoomStatus.Cleaning;

        // Create housekeeping task
        var housekeepingTask = new HousekeepingTask
        {
            RoomId = booking.RoomId,
            Status = HousekeepingStatus.Dirty,
            Notes = "Room needs cleaning after checkout"
        };
        db.HousekeepingTasks.Add(housekeepingTask);

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Booking", booking.Id, "CheckedOut", booking.UserId,
            $"BookingCode={booking.BookingCode} Room={booking.Room.RoomNumber}",
            cancellationToken);

        await notificationService.CreateAsync(
            booking.UserId,
            "Check-Out Completed",
            $"Check-out completed for booking {booking.BookingCode}.",
            NotificationType.CheckOutCompleted,
            cancellationToken);

        await publisher.Publish(new CheckOutCompletedEvent(
            booking.Id,
            booking.UserId,
            booking.User.Email,
            booking.User.FullName,
            booking.BookingCode,
            booking.Room.RoomNumber,
            booking.ActualCheckOut.Value), cancellationToken);

        logger.LogInformation(
            "Check-out completed. BookingId={BookingId} BookingCode={BookingCode} RoomId={RoomId} UserId={UserId}",
            booking.Id,
            booking.BookingCode,
            booking.RoomId,
            booking.UserId);

        return new CheckInOutResponse(
            booking.Id,
            booking.BookingCode,
            booking.Status.ToString(),
            booking.Room.RoomNumber,
            booking.Room.Status.ToString(),
            booking.ActualCheckOut.Value,
            $"Guest checked out from room {booking.Room.RoomNumber}. Room queued for cleaning.");
    }
}
