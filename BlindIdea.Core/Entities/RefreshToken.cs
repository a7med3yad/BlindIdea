using System;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Represents a refresh token used for obtaining new access tokens.
    /// Implements token rotation pattern for enhanced security.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Unique identifier for the refresh token.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Hashed refresh token value (SHA-256 hash stored in database).
        /// Plain token is never stored for security.
        /// </summary>
        public string TokenHash { get; set; } = null!;

        /// <summary>
        /// JWT ID (jti claim) of the associated access token.
        /// Used for linking tokens and audit trails.
        /// </summary>
        public string JwtId { get; set; } = null!;

        /// <summary>
        /// Foreign key to the User entity.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the User entity.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// When the refresh token expires (typically 7-30 days).
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the refresh token was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// IP address where the token was created (for audit trail).
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// When the refresh token was revoked (null if not revoked).
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// IP address where the token was revoked (for audit trail).
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// The new refresh token ID that replaced this one (token rotation).
        /// Used to detect token family for security breach detection.
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// Indicates if the token has been used (for single-use validation).
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Indicates if the token has been explicitly revoked.
        /// </summary>
        public bool IsRevoked => RevokedAt != null;

        /// <summary>
        /// Indicates if the token is still valid (not expired and not revoked).
        /// </summary>
        public bool IsValid => !IsRevoked && !IsExpired;

        /// <summary>
        /// Indicates if the token has expired based on current time.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
