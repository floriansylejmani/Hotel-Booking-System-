using FluentValidation;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Rooms.DTOs;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Application.Features.Rooms;

public sealed class RoomService(
    IAppDbContext db,
    IValidator<CreateRoomRequest> createValidator,
    IValidator<UpdateRoomRequest> updateValidator,
    IValidator<RoomFilterRequest> filterValidator,
    IAuditService auditService) : IRoomService
{
    public async Task<RoomResponse> CreateAsync(
        CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedNumber = request.RoomNumber.Trim().ToUpperInvariant();

        var exists = await db.Rooms
            .AnyAsync(r => r.RoomNumber == normalizedNumber, cancellationToken);

        if (exists)
            throw new ConflictException($"Room '{normalizedNumber}' already exists.");

        var room = new Room
        {
            RoomNumber = normalizedNumber,
            FloorNumber = request.FloorNumber,
            Type = request.Type,
            PricePerNight = request.PricePerNight,
            BedCount = request.BedCount,
            Amenities = request.Amenities
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Room", room.Id, "Created", null,
            $"RoomNumber={room.RoomNumber} Type={room.Type} Floor={room.FloorNumber}",
            cancellationToken);

        return ToResponse(room);
    }

    public async Task<RoomsPagedResult> GetAllAsync(
        RoomFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var query = db.Rooms.AsNoTracking().AsQueryable();

        if (filter.Type.HasValue)
            query = query.Where(r => r.Type == filter.Type.Value);

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (filter.Floor.HasValue)
            query = query.Where(r => r.FloorNumber == filter.Floor.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(r => r.RoomNumber.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.FloorNumber)
            .ThenBy(r => r.RoomNumber)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RoomResponse(
                r.Id, r.RoomNumber, r.FloorNumber,
                r.Type.ToString(), r.Status.ToString(),
                r.PricePerNight, r.BedCount, r.Amenities, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new RoomsPagedResult(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<RoomResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var room = await db.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Room", id);

        return ToResponse(room);
    }

    public async Task<RoomResponse> UpdateAsync(
        Guid id,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var room = await db.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Room", id);

        if (request.RoomNumber is not null)
        {
            var normalizedNumber = request.RoomNumber.Trim().ToUpperInvariant();
            if (normalizedNumber != room.RoomNumber)
            {
                var exists = await db.Rooms
                    .AnyAsync(r => r.RoomNumber == normalizedNumber, cancellationToken);
                if (exists)
                    throw new ConflictException($"Room '{normalizedNumber}' already exists.");
                room.RoomNumber = normalizedNumber;
            }
        }

        if (request.FloorNumber.HasValue) room.FloorNumber = request.FloorNumber.Value;
        if (request.Type.HasValue) room.Type = request.Type.Value;
        if (request.PricePerNight.HasValue) room.PricePerNight = request.PricePerNight.Value;
        if (request.BedCount.HasValue) room.BedCount = request.BedCount.Value;
        if (request.Amenities is not null) room.Amenities = request.Amenities;
        if (request.Status.HasValue) room.Status = request.Status.Value;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Room", room.Id, "Updated", null,
            $"RoomNumber={room.RoomNumber}",
            cancellationToken);

        return ToResponse(room);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await db.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Room", id);

        var hasActiveBookings = await db.Bookings.AnyAsync(
            b => b.RoomId == id &&
                 b.Status != BookingStatus.Cancelled &&
                 b.Status != BookingStatus.CheckedOut,
            cancellationToken);

        if (hasActiveBookings)
            throw new ConflictException(
                $"Room '{room.RoomNumber}' cannot be deleted because it has active or confirmed bookings.");

        var roomId = room.Id;
        var roomNumber = room.RoomNumber;
        db.Rooms.Remove(room);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Room", roomId, "Deleted", null,
            $"RoomNumber={roomNumber}",
            cancellationToken);
    }

    private static RoomResponse ToResponse(Room room) => new(
        room.Id, room.RoomNumber, room.FloorNumber,
        room.Type.ToString(), room.Status.ToString(),
        room.PricePerNight, room.BedCount, room.Amenities, room.CreatedAt);
}
