using System;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Represents an email verification token sent to users during registration.
    /// Used to verify user's email address before allowing full access.
    /// Tokens expire after 24 hours and can only be used once.
    /// </summary>
    public class EmailVerificationToken : BaseEntity
    {
        /// <summary>
        /// Hashed token value (SHA-256 hash stored in database).
        /// Plain tokens are never stored for security.
        /// </summary>
        public string TokenHash { get; set; } = null!;

        /// <summary>
        /// Foreign key to the User entity.
        /// Identifies which user this token is for.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the User entity.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// When the verification token expires (typically 24 hours).
        /// After this time, token is no longer valid and user must request a new one.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the email was verified (null if not yet verified).
        /// Indicates successful verification attempt.
        /// </summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// Indicates if the token has been used to verify email.
        /// Computed based on VerifiedAt property.
        /// </summary>
        public bool IsUsed => VerifiedAt != null;

        /// <summary>
        /// Indicates if the token is still valid (not expired and not used).
        /// Used to determine if token can still be used for verification.
        /// </summary>
        public bool IsValid => !IsExpired && !IsUsed;

        /// <summary>
        /// Indicates if the token has expired based on current time.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        public EmailVerificationToken() { }

        /// <summary>
        /// Creates a new email verification token.
        /// </summary>
        /// <param name="userId">User ID to verify</param>
        /// <param name="tokenHash">SHA-256 hash of the token</param>
        /// <param name="expiresAt">When the token expires</param>
        public EmailVerificationToken(string userId, string tokenHash, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the token as verified (email address confirmed).
        /// </summary>
        public void MarkAsVerified()
        {
            VerifiedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates that the token can still be used for verification.
        /// </summary>
        /// <returns>True if token is valid for use, false otherwise</returns>
        public bool ValidateForUse()
        {
            return IsValid && !IsExpired && !IsUsed;
        }
    }
}
