namespace BlindIdea.Core.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string userId, string email, string verificationToken, CancellationToken cancellationToken = default);
}
