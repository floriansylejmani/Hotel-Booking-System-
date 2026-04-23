using System.Net;
using System.Net.Mail;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HotelBooking.Infrastructure.Services;

public sealed class SmtpEmailService(
    IOptions<SmtpSettings> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            logger.LogDebug("SMTP not configured — skipping email to {To}: {Subject}", to, subject);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(to);

#pragma warning disable CS0618
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
            await client.SendMailAsync(message, ct);
#pragma warning restore CS0618

            logger.LogInformation("Email sent To={To} Subject={Subject}", to, subject);
        }
        catch (Exception ex)
        {
            // Email failure must never break the primary operation
            logger.LogError(ex, "Failed to send email To={To} Subject={Subject}", to, subject);
        }
    }
}
