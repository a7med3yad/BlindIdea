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
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendVerificationEmailAsync(string userId, string email, string verificationToken, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.AppBaseUrl?.TrimEnd('/') ?? "https://localhost:7024";
        var encodedToken = Uri.EscapeDataString(verificationToken);
        var encodedUserId = Uri.EscapeDataString(userId);
        var verificationUrl = $"{baseUrl}/api/auth/verify-email?userId={encodedUserId}&token={encodedToken}";

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>Welcome to BlindIdea!</h2>
                <p>Thank you for registering. Please verify your email address by clicking the button below:</p>
                <p style='text-align: center; margin: 30px 0;'>
                    <a href=""{verificationUrl}"" 
                       style='background-color: #4CAF50; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-size: 16px;'>
                        Verify Email Address
                    </a>
                </p>
                <p>Or copy and paste this link into your browser:</p>
                <p style='word-break: break-all; color: #666;'>{verificationUrl}</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
                <p style='color: #999; font-size: 12px;'>This link will expire in 60 minutes. If you did not register for BlindIdea, please ignore this email.</p>
            </div>";

        await SendEmailAsync(email, "Verify your email - BlindIdea", htmlBody, cancellationToken);
    }
}
