using HotelBooking.Application.Features.Dashboard.DTOs;
using MediatR;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardOccupancy;

public sealed record GetDashboardOccupancyQuery : IRequest<DashboardOccupancyResponse>;
