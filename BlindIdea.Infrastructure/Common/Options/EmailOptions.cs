using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Common.Options
{
    /// <summary>
    /// Email configuration options.
    /// Binds from appsettings.json "Email" section.
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// Sender email address.
        /// </summary>
        public required string From { get; set; }

        /// <summary>
        /// SMTP server hostname.
        /// </summary>
        public required string SmtpServer { get; set; }

        /// <summary>
        /// SMTP server port (typically 587 for TLS, 465 for SSL).
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// SMTP authentication username.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// SMTP authentication password.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Whether to use SSL/TLS encryption.
        /// </summary>
        public bool EnableSsl { get; set; } = true;
    }
}

