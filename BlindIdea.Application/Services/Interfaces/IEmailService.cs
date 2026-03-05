using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Service for sending transactional emails.
    /// Handles email verification, password reset, and notifications.
    /// </summary>
    public interface IEmailService
    {
        // ===== CORE EMAIL SENDING =====

        /// <summary>
        /// Sends a plain text email.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject line</param>
        /// <param name="message">Plain text message body</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);

        /// <summary>
        /// Sends an HTML email.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject line</param>
        /// <param name="htmlContent">HTML email body</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendHtmlEmailAsync(string toEmail, string subject, string htmlContent);

        /// <summary>
        /// Sends email to multiple recipients.
        /// </summary>
        /// <param name="toEmails">List of recipient email addresses</param>
        /// <param name="subject">Email subject line</param>
        /// <param name="htmlContent">HTML email body</param>
        /// <returns>True if all emails sent successfully</returns>
        Task<bool> SendBulkEmailAsync(List<string> toEmails, string subject, string htmlContent);

        // ===== AUTHENTICATION EMAILS =====

        /// <summary>
        /// Sends email verification link to new users.
        /// User must click link to activate account.
        /// </summary>
        /// <param name="toEmail">User's email address</param>
        /// <param name="userName">User's name for personalization</param>
        /// <param name="verificationUrl">URL with embedded verification token</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendEmailVerificationAsync(string toEmail, string userName, string verificationUrl);

        /// <summary>
        /// Sends password reset link.
        /// User uses link to securely reset password.
        /// </summary>
        /// <param name="toEmail">User's email address</param>
        /// <param name="userName">User's name</param>
        /// <param name="resetUrl">URL with embedded password reset token</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendPasswordResetAsync(string toEmail, string userName, string resetUrl);

        /// <summary>
        /// Confirms password change to user.
        /// Sent after password is successfully changed.
        /// </summary>
        /// <param name="toEmail">User's email address</param>
        /// <param name="userName">User's name</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendPasswordChangedConfirmationAsync(string toEmail, string userName);

        /// <summary>
        /// Alerts user of suspicious login attempt.
        /// Sent when login from new device/location detected.
        /// </summary>
        /// <param name="toEmail">User's email address</param>
        /// <param name="userName">User's name</param>
        /// <param name="ipAddress">IP address of login attempt</param>
        /// <param name="timestamp">When the login occurred</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendSuspiciousActivityAlertAsync(
            string toEmail, string userName, string? ipAddress, DateTime timestamp);

        // ===== NOTIFICATION EMAILS =====

        /// <summary>
        /// Notifies user when someone rates their idea.
        /// </summary>
        /// <param name="toEmail">User's email address</param>
        /// <param name="userName">User's name</param>
        /// <param name="raterName">Name of person who rated</param>
        /// <param name="ideaTitle">Title of the idea</param>
        /// <param name="rating">Rating value (1-5)</param>
        /// <param name="ideaUrl">URL to view the idea</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendIdeaRatedNotificationAsync(
            string toEmail, string userName, string raterName, string ideaTitle, int rating, string ideaUrl);

        /// <summary>
        /// Notifies team members of new idea in team.
        /// </summary>
        /// <param name="toEmail">Recipient email</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="ideaCreator">Name of idea creator</param>
        /// <param name="ideaTitle">Title of the idea</param>
        /// <param name="teamName">Name of the team</param>
        /// <param name="ideaUrl">URL to view the idea</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendNewIdeaNotificationAsync(
            string toEmail, string recipientName, string ideaCreator, string ideaTitle, string teamName, string ideaUrl);

        /// <summary>
        /// Notifies user of new team membership.
        /// </summary>
        /// <param name="toEmail">User's email address</param>
        /// <param name="userName">User's name</param>
        /// <param name="teamName">Name of the team</param>
        /// <param name="addedBy">Name of person who added user</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> SendTeamInvitationAsync(string toEmail, string userName, string teamName, string addedBy);

        // ===== UTILITY =====

        /// <summary>
        /// Tests email configuration by sending test email.
        /// </summary>
        /// <param name="toEmail">Email address to send test to</param>
        /// <returns>True if test email sent successfully</returns>
        Task<bool> SendTestEmailAsync(string toEmail);

        /// <summary>
        /// Validates email address format.
        /// </summary>
        /// <param name="email">Email address to validate</param>
        /// <returns>True if valid email format</returns>
        bool IsValidEmailFormat(string email);
    }
}
