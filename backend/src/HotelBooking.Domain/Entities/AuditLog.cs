namespace HotelBooking.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string EntityName { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Action { get; init; } = string.Empty;
    public Guid? ActorId { get; init; }
    public string? Details { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
