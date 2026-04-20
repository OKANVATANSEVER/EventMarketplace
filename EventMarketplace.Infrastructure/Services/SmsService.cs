using EventMarketplace.Application.Services;
using Microsoft.Extensions.Logging;

namespace EventMarketplace.Infrastructure.Services;

/// <summary>
/// SMS service stub. Logs messages to console.
/// For production, replace with Twilio or Netgsm implementation.
/// </summary>
public sealed class SmsService(ILogger<SmsService> logger) : ISmsService
{
    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // TODO: Integrate with Twilio / Netgsm / Vonage for production SMS delivery
        logger.LogInformation("[SMS STUB] To: {Phone} | Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
