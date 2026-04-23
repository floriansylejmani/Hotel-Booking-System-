using FluentValidation;
using HotelBooking.Application.Common.Behaviors;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Audit;
using HotelBooking.Application.Features.Auth;
using HotelBooking.Application.Features.Bookings;
using HotelBooking.Application.Features.CheckInOut;
using HotelBooking.Application.Features.Housekeeping;
using HotelBooking.Application.Features.Notifications;
using HotelBooking.Application.Features.Payments;
using HotelBooking.Application.Features.Roles;
using HotelBooking.Application.Features.Rooms;
using HotelBooking.Application.Features.Staff;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ICheckInOutService, CheckInOutService>();
        services.AddScoped<IHousekeepingService, HousekeepingService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IStaffService, StaffService>();

        return services;
    }
}
