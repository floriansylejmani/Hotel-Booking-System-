using System.Text;
using HotelBooking.Domain.Constants;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Settings;
using HotelBooking.Infrastructure.Services;
using HotelBooking.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");

        services.AddOptions<JwtSettings>()
            .Bind(jwtSection)
            .Validate(settings =>
                !string.IsNullOrWhiteSpace(settings.Key) &&
                settings.Key.Length >= 32 &&
                !string.IsNullOrWhiteSpace(settings.Issuer) &&
                !string.IsNullOrWhiteSpace(settings.Audience) &&
                settings.ExpirationMinutes > 0,
                "JwtSettings configuration is invalid.")
            .ValidateOnStart();

        services.AddOptions<TaxSettings>()
            .Bind(configuration.GetSection("TaxSettings"))
            .Validate(settings => settings.Rate >= 0m && settings.Rate <= 1m,
                "TaxSettings:Rate must be between 0.00 and 1.00.")
            .ValidateOnStart();

        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection("SmtpSettings"));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration is missing.");
        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.Admin))
            .AddPolicy("ManagerOrAdmin", p => p.RequireRole(RoleNames.Admin, RoleNames.Manager))
            .AddPolicy(
                "ReceptionistOrAbove",
                p => p.RequireRole(RoleNames.Admin, RoleNames.Manager, RoleNames.Receptionist))
            .AddPolicy(
                "StaffOrAbove",
                p => p.RequireRole(
                    RoleNames.Admin,
                    RoleNames.Manager,
                    RoleNames.Receptionist,
                    RoleNames.Housekeeper))
            .AddPolicy(
                "HousekeeperOrAbove",
                p => p.RequireRole(
                    RoleNames.Admin,
                    RoleNames.Manager,
                    RoleNames.Housekeeper));

        return services;
    }
}
