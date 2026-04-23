using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Common;

public static class ApiResponseFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static ApiResponse Success(HttpContext context, object? data, string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = string.IsNullOrWhiteSpace(message)
                ? GetDefaultSuccessMessage(context.Response.StatusCode, context.Request.Method)
                : message,
            Data = data,
            TraceId = context.TraceIdentifier
        };
    }

    public static ApiResponse Error(
        HttpContext context,
        string message,
        IDictionary<string, string[]>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            TraceId = context.TraceIdentifier
        };
    }

    public static IActionResult ValidationProblem(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => string.IsNullOrWhiteSpace(entry.Key) ? "request" : entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The provided value is invalid."
                        : error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new BadRequestObjectResult(Error(
            context.HttpContext,
            "One or more validation errors occurred.",
            errors));
    }

    public static Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message,
        IDictionary<string, string[]>? errors = null)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(JsonSerializer.Serialize(
            Error(context, message, errors),
            SerializerOptions));
    }

    private static string GetDefaultSuccessMessage(int statusCode, string method)
    {
        return statusCode switch
        {
            StatusCodes.Status201Created => "Resource created successfully.",
            StatusCodes.Status202Accepted => "Request accepted successfully.",
            _ => HttpMethods.IsGet(method)
                ? "Request completed successfully."
                : "Operation completed successfully."
        };
    }
}
