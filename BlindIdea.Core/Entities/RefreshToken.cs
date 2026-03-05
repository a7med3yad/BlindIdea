using System;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Represents a refresh token used for obtaining new access tokens.
    /// Implements token rotation pattern for enhanced security.
    /// Tokens are hashed for storage and have audit trails.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Hashed refresh token value (SHA-256 hash stored in database).
        /// Plain token is never stored for security.
        /// </summary>
        public string TokenHash { get; set; } = null!;

        /// <summary>
        /// JWT ID (jti claim) of the associated access token.
        /// Used for linking tokens and audit trails.
        /// Helps detect token families for security breach detection.
        /// </summary>
        public string JwtId { get; set; } = null!;

        /// <summary>
        /// Foreign key to the User entity.
        /// Links the refresh token to its owner.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the User entity.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// When the refresh token expires (typically 7-30 days).
        /// Automatically validated during token refresh.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// IP address where the token was created (for audit trail).
        /// Helps track device/location of token generation.
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// When the refresh token was revoked (null if not revoked).
        /// Revocation can be explicit (user logout) or automatic (rotation).
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// IP address where the token was revoked (for audit trail).
        /// Null if token was never revoked.
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// The new refresh token ID that replaced this one (token rotation).
        /// Used to detect token family for security breach detection.
        /// If non-null, indicates this token was rotated and replaced.
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// Indicates if the token has been used (for single-use validation).
        /// Used tokens should not be reused even if not revoked.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Indicates if the token has been explicitly revoked.
        /// Computed based on RevokedAt property.
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

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        public RefreshToken() { }

        /// <summary>
        /// Creates a new refresh token with provided details.
        /// </summary>
        /// <param name="userId">User ID who owns the token</param>
        /// <param name="tokenHash">SHA-256 hash of the token</param>
        /// <param name="jwtId">JWT ID (jti) from access token</param>
        /// <param name="expiresAt">Expiration datetime</param>
        /// <param name="ipAddress">IP address creating the token</param>
        public RefreshToken(string userId, string tokenHash, string jwtId, DateTime expiresAt, string? ipAddress = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            JwtId = jwtId;
            ExpiresAt = expiresAt;
            CreatedByIp = ipAddress;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Revokes this token (marks as revoked).
        /// </summary>
        /// <param name="ipAddress">IP address revoking the token</param>
        public void Revoke(string? ipAddress = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = ipAddress;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks this token as replaced by a new token (rotation).
        /// </summary>
        /// <param name="newTokenId">ID of the new replacement token</param>
        public void ReplaceWith(Guid newTokenId)
        {
            ReplacedByTokenId = newTokenId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks this token as used.
        /// </summary>
        public void MarkAsUsed()
        {
            IsUsed = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

