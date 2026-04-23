using HotelBooking.Application.Features.Dashboard.DTOs;
using MediatR;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardActivityFeed;

public sealed record GetDashboardActivityFeedQuery(int Count = 6)
    : IRequest<DashboardActivityFeedResponse>;
