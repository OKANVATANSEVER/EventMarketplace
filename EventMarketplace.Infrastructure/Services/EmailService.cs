using EventMarketplace.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EventMarketplace.Infrastructure.Services;

public sealed class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly string _host = configuration["Email:Host"] ?? "smtp.gmail.com";
    private readonly int _port = int.Parse(configuration["Email:Port"] ?? "587");
    private readonly string _username = configuration["Email:Username"] ?? string.Empty;
    private readonly string _password = configuration["Email:Password"] ?? string.Empty;
    private readonly string _fromEmail = configuration["Email:FromEmail"] ?? "noreply@eventmarketplace.com";
    private readonly string _fromName = configuration["Email:FromName"] ?? "EventMarketplace";

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            logger.LogWarning("Email service is not configured. Skipping send to {Email}", toEmail);
            return;
        }

        try
        {
            var message = BuildMessage([(toEmail, toName)], subject, htmlBody);
            await SendMessageAsync(message, cancellationToken);
            logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    public async Task SendBulkAsync(
        IEnumerable<(string Email, string Name)> recipients,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            logger.LogWarning("Email service is not configured. Skipping bulk send.");
            return;
        }

        var recipientList = recipients.ToList();
        if (recipientList.Count == 0)
            return;

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_username, _password, cancellationToken);

            // Send in batches of 50 to avoid SMTP limits
            foreach (var batch in recipientList.Chunk(50))
            {
                var message = BuildMessage(batch, subject, htmlBody);
                await smtp.SendAsync(message, cancellationToken);
            }

            await smtp.DisconnectAsync(true, cancellationToken);
            logger.LogInformation("Bulk email sent to {Count} recipients", recipientList.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send bulk email to {Count} recipients", recipientList.Count);
        }
    }

    private MimeMessage BuildMessage(IEnumerable<(string Email, string Name)> toList, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));

        foreach (var (email, name) in toList)
            message.To.Add(new MailboxAddress(name, email));

        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };
        return message;
    }

    private async Task SendMessageAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(_username, _password, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}
