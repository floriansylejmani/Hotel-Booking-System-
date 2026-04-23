namespace HotelBooking.Application.Features.Dashboard.DTOs;

public sealed record DashboardSummaryResponse(
    int TotalRooms,
    int AvailableRooms,
    int OccupiedRooms,
    int CleaningRooms,
    int MaintenanceRooms,
    int TotalBookings,
    int ActiveBookings,
    int ConfirmedBookings,
    decimal MonthlyRevenue,
    int UnreadNotificationsCount);
