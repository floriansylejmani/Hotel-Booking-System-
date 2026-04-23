using HotelBooking.Application.Features.Staff.DTOs;

namespace HotelBooking.Application.Features.Staff;

public interface IStaffService
{
    Task<StaffPagedResult> GetAllAsync(StaffFilterRequest filter, CancellationToken cancellationToken = default);
    Task<StaffResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken = default);
    Task<StaffResponse> UpdateAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken = default);
    Task<StaffResponse> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HousekeeperOption>> GetAssignableHousekeepersAsync(CancellationToken cancellationToken = default);
}
