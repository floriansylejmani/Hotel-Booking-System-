namespace HotelBooking.Application.Features.Dashboard.DTOs;

public sealed record DashboardActivityFeedResponse(
    IReadOnlyList<ActivityFeedItem> Items);

public sealed record ActivityFeedItem(
    Guid Id,
    string Title,
    string Description,
    string Type,
    DateTime CreatedAt);
