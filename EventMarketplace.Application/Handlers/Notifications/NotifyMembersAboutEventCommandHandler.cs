using EventMarketplace.Application.Repositories;
using EventMarketplace.Application.Services;
using MediatR;

namespace EventMarketplace.Application.Commands.Notifications;

public class NotifyMembersAboutEventCommandHandler(
    IUserRepository userRepository,
    IEmailService emailService)
    : IRequestHandler<NotifyMembersAboutEventCommand, bool>
{
    public async Task<bool> Handle(
        NotifyMembersAboutEventCommand request,
        CancellationToken cancellationToken)
    {
        var recipients = await userRepository.GetAllMemberEmailsAsync(cancellationToken);

        if (!recipients.Any())
            return false;

        var subject = $"Yeni Etkinlik: {request.EventTitle}";
        var htmlBody = BuildEmailHtml(request);

        await emailService.SendBulkAsync(recipients, subject, htmlBody, cancellationToken);

        return true;
    }

    private static string BuildEmailHtml(NotifyMembersAboutEventCommand e) => $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8" /></head>
        <body style="font-family: Arial, sans-serif; background:#f5f5f5; padding:20px;">
          <div style="max-width:600px;margin:auto;background:#fff;border-radius:8px;overflow:hidden;">
            <div style="background:#6366f1;padding:24px;color:#fff;">
              <h1 style="margin:0;font-size:24px;">🎉 Yeni Etkinlik Duyurusu</h1>
            </div>
            <div style="padding:24px;">
              <h2 style="color:#1e293b;">{e.EventTitle}</h2>
              <p style="color:#475569;">{e.EventDescription}</p>
              <table style="width:100%;border-collapse:collapse;margin-top:16px;">
                <tr>
                  <td style="padding:8px;background:#f8fafc;font-weight:bold;">📅 Tarih</td>
                  <td style="padding:8px;">{e.StartDate:dd MMMM yyyy HH:mm}</td>
                </tr>
                <tr>
                  <td style="padding:8px;background:#f8fafc;font-weight:bold;">📍 Şehir</td>
                  <td style="padding:8px;">{e.City}</td>
                </tr>
                <tr>
                  <td style="padding:8px;background:#f8fafc;font-weight:bold;">💰 Fiyat</td>
                  <td style="padding:8px;">{(e.Price == 0 ? "Ücretsiz" : $"{e.Price:C2}")}</td>
                </tr>
              </table>
              <div style="margin-top:24px;text-align:center;">
                <a href="#" style="background:#6366f1;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;">
                  Etkinliği İncele
                </a>
              </div>
            </div>
            <div style="padding:16px;background:#f8fafc;text-align:center;color:#94a3b8;font-size:12px;">
              © {DateTime.UtcNow.Year} EventMarketplace – Bu e-postayı almak istemiyorsanız lütfen bize ulaşın.
            </div>
          </div>
        </body>
        </html>
        """;
}
