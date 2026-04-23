using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Audit;

public sealed class AuditService(
    IAppDbContext db,
    ILogger<AuditService> logger) : IAuditService
{
    public async Task LogAsync(
        string entityName,
        Guid entityId,
        string action,
        Guid? actorId = null,
        string? details = null,
        CancellationToken ct = default)
    {
        try
        {
            db.AuditLogs.Add(new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                ActorId = actorId,
                Details = details,
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Audit failure must never propagate to the caller
            logger.LogError(ex,
                "Failed to write audit log. Entity={Entity} EntityId={EntityId} Action={Action}",
                entityName, entityId, action);
        }
    }
}
