using BlindIdea.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace BlindIdea.Application.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();

            // Sender
            email.From.Add(new MailboxAddress(
                "BlindIdea Support",
                _configuration["Email:From"]
            ));

            // Recipient
            email.To.Add(new MailboxAddress("", toEmail));

            // Subject & Body
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = message };

            using var smtp = new SmtpClient();
            try
            {
                // Connect
                await smtp.ConnectAsync(
                    _configuration["Email:SmtpServer"],
                    int.Parse(_configuration["Email:Port"]),
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                // Authenticate
                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                // Send
                await smtp.SendAsync(email);
            }
            finally
            {
                // Always disconnect
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
