using System.Net;
using FluentValidation;
using HotelBooking.API.Common;
using HotelBooking.Application.Common.Exceptions;

namespace HotelBooking.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                validationException.Errors
                    .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray())
            ),
            NotFoundException => (
                (int)HttpStatusCode.NotFound,
                exception.Message,
                (IDictionary<string, string[]>?)null
            ),
            ConflictException => (
                (int)HttpStatusCode.Conflict,
                exception.Message,
                (IDictionary<string, string[]>?)null
            ),
            ForbiddenException => (
                (int)HttpStatusCode.Forbidden,
                exception.Message,
                (IDictionary<string, string[]>?)null
            ),
            UnauthorizedException => (
                (int)HttpStatusCode.Unauthorized,
                exception.Message,
                (IDictionary<string, string[]>?)null
            ),
            BadRequestException => (
                (int)HttpStatusCode.BadRequest,
                exception.Message,
                (IDictionary<string, string[]>?)null
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                environment.IsDevelopment() ? exception.Message : "An unexpected server error occurred.",
                (IDictionary<string, string[]>?)null
            ),
        };

        logger.LogError(
            exception,
            "Unhandled exception. TraceId={TraceId} StatusCode={StatusCode} Method={Method} Path={Path}",
            traceId,
            statusCode,
            context.Request.Method,
            context.Request.Path);

        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "The response has already started, skipping exception response body. TraceId={TraceId}",
                traceId);
            return;
        }

        await ApiResponseFactory.WriteErrorAsync(context, statusCode, message, errors);
    }
}
