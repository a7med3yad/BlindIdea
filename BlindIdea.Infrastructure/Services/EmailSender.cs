using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BlindIdea.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailOptions> options, ILogger<EmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_options.SmtpServer, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            var message = new MailMessage(_options.From ?? "", to, subject, htmlBody) { IsBodyHtml = true };
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendVerificationEmailAsync(string email, string verificationToken, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.AppBaseUrl?.TrimEnd('/') ?? "http://localhost:5000";
        var verificationUrl = $"{baseUrl}/api/auth/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";
        var htmlBody = $@"
            <h2>Verify your email</h2>
            <p>Please click the link below to verify your email address:</p>
            <p><a href=""{verificationUrl}"">Verify Email</a></p>
            <p>This link will expire in 60 minutes.</p>
        ";
        await SendEmailAsync(email, "Verify your email - BlindIdea", htmlBody, cancellationToken);
    }
}
