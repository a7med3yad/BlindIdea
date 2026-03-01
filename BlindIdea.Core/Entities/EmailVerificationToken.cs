using System;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Represents an email verification token sent to users during registration.
    /// </summary>
    public class EmailVerificationToken
    {
        /// <summary>
        /// Unique identifier for the email verification token.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Hashed token value (SHA-256 hash stored in database).
        /// </summary>
        public string TokenHash { get; set; } = null!;

        /// <summary>
        /// Foreign key to the User entity.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the User entity.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// When the verification token expires (typically 24 hours).
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the verification token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the email was verified (null if not yet verified).
        /// </summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// Indicates if the token has been used to verify email.
        /// </summary>
        public bool IsUsed => VerifiedAt != null;

        /// <summary>
        /// Indicates if the token is still valid (not expired and not used).
        /// </summary>
        public bool IsValid => !IsExpired && !IsUsed;

        /// <summary>
        /// Indicates if the token has expired based on current time.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
