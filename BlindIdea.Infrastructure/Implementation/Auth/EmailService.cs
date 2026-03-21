using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using BlindIdea.Domain.Abstraction.Services;
using Microsoft.Extensions.Configuration;

namespace BlindIdea.Infrastructure.Implementation.Auth
{
    public class EmailService : IEmailService
    {
        private readonly string _email;
        private readonly string _password;
        private readonly string _displayName;

        public EmailService(IConfiguration config)
        {
            _email = config["EmailSettings:Email"]
                ?? throw new Exception("Email not configured");
            _password = config["EmailSettings:Password"]
                ?? throw new Exception("Email password not configured");
            _displayName = config["EmailSettings:DisplayName"] ?? "BlindIdea";
        }

        public async Task SendOtp(string email, string otp)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_displayName, _email));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Your BlindIdea Verification Code";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                </head>
                <body style="margin:0; padding:0; background-color:#000000; font-family:'Segoe UI', Arial, sans-serif;">
                    
                    <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#000000; padding: 40px 0;">
                        <tr>
                            <td align="center">
                                <table width="600" cellpadding="0" cellspacing="0" style="background-color:#0D0D0D; border-radius:12px; border: 1px solid #2A2A2A; overflow:hidden;">
                                    
                                    <!-- Header -->
                                    <tr>
                                        <td align="center" style="background-color:#000000; padding: 32px 40px; border-bottom: 2px solid #E8003D;">
                                            <h1 style="margin:0; font-size:36px; letter-spacing:2px;">
                                                <span style="color:#E8003D; font-weight:900;">Blind</span>
                                                <span style="color:#FFFFFF; font-weight:900;">Idea</span>
                                            </h1>
                                            <p style="margin: 6px 0 0 0; color:#E8003D; font-size:12px; letter-spacing:3px; text-transform:uppercase;">
                                                Innovation without ego.
                                            </p>
                                        </td>
                                    </tr>

                                    <!-- Body -->
                                    <tr>
                                        <td style="padding: 40px 48px;">
                                            
                                            <p style="color:#FFFFFF; font-size:16px; margin: 0 0 8px 0;">
                                                Hello,
                                            </p>
                                            <p style="color:#AAAAAA; font-size:15px; margin: 0 0 32px 0; line-height:1.6;">
                                                Use the verification code below to complete your action. 
                                                This code is valid for <span style="color:#E8003D; font-weight:600;">5 minutes</span>.
                                            </p>

                                            <!-- OTP Box -->
                                            <table width="100%" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td align="center" style="padding: 0 0 32px 0;">
                                                        <div style="
                                                            display:inline-block;
                                                            background-color:#1A1A1A;
                                                            border: 2px solid #E8003D;
                                                            border-radius:12px;
                                                            padding: 20px 48px;
                                                        ">
                                                            <p style="margin:0; font-size:11px; color:#AAAAAA; letter-spacing:3px; text-transform:uppercase; margin-bottom:8px;">
                                                                Verification Code
                                                            </p>
                                                            <p style="margin:0; font-size:42px; font-weight:900; letter-spacing:10px; color:#E8003D;">
                                                                {otp}
                                                            </p>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>

                                            <!-- Warning -->
                                            <table width="100%" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td style="
                                                        background-color:#1A1A1A;
                                                        border-left: 3px solid #E8003D;
                                                        border-radius: 4px;
                                                        padding: 14px 16px;
                                                        margin-bottom: 32px;
                                                    ">
                                                        <p style="margin:0; color:#AAAAAA; font-size:13px; line-height:1.6;">
                                                            ⚠️ <strong style="color:#FFFFFF;">Never share this code</strong> with anyone. 
                                                            BlindIdea will never ask for your OTP via phone or chat.
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style="color:#555555; font-size:13px; margin: 32px 0 0 0; line-height:1.6;">
                                                If you did not request this code, you can safely ignore this email. 
                                                Someone may have entered your email by mistake.
                                            </p>

                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td style="background-color:#000000; padding: 24px 48px; border-top: 1px solid #2A2A2A;">
                                            <table width="100%" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td>
                                                        <p style="margin:0; color:#555555; font-size:12px;">
                                                            © 2026 
                                                            <span style="color:#E8003D;">Blind</span><span style="color:#FFFFFF;">Idea</span>
                                                            · All rights reserved.
                                                        </p>
                                                        <p style="margin: 4px 0 0 0; color:#555555; font-size:12px;">
                                                            Innovation without ego.
                                                        </p>
                                                    </td>
                                                    <td align="right">
                                                        <p style="margin:0; color:#555555; font-size:12px;">
                                                            Do not reply to this email.
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>

                                </table>
                            </td>
                        </tr>
                    </table>

                </body>
                </html>
                """
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_email, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
