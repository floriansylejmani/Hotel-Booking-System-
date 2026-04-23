using HotelBooking.Application.Features.Rooms.DTOs;

namespace HotelBooking.Application.Common.Interfaces;

public interface IRoomService
{
    Task<RoomResponse> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomsPagedResult> GetAllAsync(RoomFilterRequest filter, CancellationToken cancellationToken = default);
    Task<RoomResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomResponse> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
