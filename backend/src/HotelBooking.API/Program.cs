using System.Security.Claims;
using System.Text.Json.Serialization;
using HotelBooking.API.Common;
using HotelBooking.API.Common.HealthChecks;
using HotelBooking.API.Filters;
using HotelBooking.API.Middleware;
using HotelBooking.Application;
using HotelBooking.Infrastructure;
using HotelBooking.Persistence;
using HotelBooking.Persistence.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;

try
{
    Log.Information("Starting Hotel Booking API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine("logs", "hotel-booking-api-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14);
    });

    builder.Services
        .AddControllers(options => options.Filters.Add<ApiSuccessEnvelopeFilter>())
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = ApiResponseFactory.ValidationProblem;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Hotel Booking API",
            Version = "v1",
            Description = "Production-ready API for hotel operations including auth, rooms, bookings, housekeeping, payments, invoices, and notifications."
        });
        c.SupportNonNullableReferenceTypes();
        c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
        c.CustomOperationIds(api =>
        {
            var controller = api.ActionDescriptor.RouteValues["controller"] ?? "Api";
            var path = (api.RelativePath ?? string.Empty)
                .Replace("/", "_")
                .Replace("{", string.Empty)
                .Replace("}", string.Empty);
            return $"{controller}_{api.HttpMethod}_{path}";
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);

    builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await ApiResponseFactory.WriteErrorAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "Authentication is required to access this resource.");
            },
            OnForbidden = async context =>
            {
                await ApiResponseFactory.WriteErrorAsync(
                    context.HttpContext,
                    StatusCodes.Status403Forbidden,
                    "You do not have permission to perform this action.");
            }
        };
    });

    var allowedOrigins = builder.Configuration
        .GetSection("CorsSettings:AllowedOrigins")
        .Get<string[]>();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (allowedOrigins is { Length: > 0 })
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            else
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });

    var app = builder.Build();

    app.Logger.LogInformation("Environment: {EnvironmentName}", app.Environment.EnvironmentName);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        };
    });

    app.UseGlobalExceptionHandling();
    app.UseCors();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    }).AllowAnonymous();
    app.MapControllers();

    app.Logger.LogInformation("Initializing database");
    await app.Services.InitializeDatabaseAsync();
    app.Logger.LogInformation("Database initialization complete");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Hotel Booking API terminated unexpectedly during startup");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
