namespace EventMarketplace.Application.Services;

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendBulkAsync(IEnumerable<(string Email, string Name)> recipients, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
