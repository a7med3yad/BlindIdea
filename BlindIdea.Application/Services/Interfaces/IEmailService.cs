using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
        Task<bool> SendHtmlEmailAsync(string toEmail, string subject, string htmlContent);
        Task<bool> SendBulkEmailAsync(List<string> toEmails, string subject, string htmlContent);
        Task<bool> SendEmailVerificationAsync(string toEmail, string userName, string verificationUrl);
        Task<bool> SendPasswordResetAsync(string toEmail, string userName, string resetUrl);
        Task<bool> SendPasswordChangedConfirmationAsync(string toEmail, string userName);
        Task<bool> SendSuspiciousActivityAlertAsync(
            string toEmail, string userName, string? ipAddress, DateTime timestamp);
        Task<bool> SendIdeaRatedNotificationAsync(
            string toEmail, string userName, string raterName, string ideaTitle, int rating, string ideaUrl);
        Task<bool> SendNewIdeaNotificationAsync(
            string toEmail, string recipientName, string ideaCreator, string ideaTitle, string teamName, string ideaUrl);
        Task<bool> SendTeamInvitationAsync(string toEmail, string userName, string teamName, string addedBy);
        Task<bool> SendTestEmailAsync(string toEmail);
        bool IsValidEmailFormat(string email);
    }
}