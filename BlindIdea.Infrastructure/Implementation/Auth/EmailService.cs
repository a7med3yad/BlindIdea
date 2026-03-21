using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using BlindIdea.Domain.Abstraction.Services;
namespace BlindIdea.Infrastructure.Implementation.Auth
{
    public class EmailService:IEmailService
    {
        public async Task SendOtp(string email, string otp)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Auth API", "ahmed.ibrahim01974@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "OTP verification";
            message.Body = new TextPart("plain")
            {
                Text = $"Your OTP is: {otp}"
            };
            using var client = new SmtpClient();

            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("ahmed.ibrahim01974@gmail.com" , "rexy wofu bsqf tcqd");
            await client.SendAsync(message); 
            await client.DisconnectAsync(true);
        }

    }
}
