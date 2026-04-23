namespace HotelBooking.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string entityName,
        Guid entityId,
        string action,
        Guid? actorId = null,
        string? details = null,
        CancellationToken ct = default);
}
