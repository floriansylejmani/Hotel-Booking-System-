namespace HotelBooking.API.Common;

public sealed class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
    public string TraceId { get; init; } = string.Empty;
}
