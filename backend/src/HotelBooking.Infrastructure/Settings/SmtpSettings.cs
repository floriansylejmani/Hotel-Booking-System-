namespace HotelBooking.Infrastructure.Settings;

public sealed class SmtpSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = "noreply@hotel.local";
    public string FromName { get; init; } = "Hotel Booking System";
    public bool EnableSsl { get; init; } = true;
}
