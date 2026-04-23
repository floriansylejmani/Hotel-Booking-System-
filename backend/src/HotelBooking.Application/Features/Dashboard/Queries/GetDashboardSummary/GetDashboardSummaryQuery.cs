using HotelBooking.Application.Features.Dashboard.DTOs;
using MediatR;

namespace HotelBooking.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(Guid CallerId) : IRequest<DashboardSummaryResponse>;
