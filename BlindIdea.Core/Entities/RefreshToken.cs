using System;

namespace BlindIdea.Core.Entities
{
    
    public class RefreshToken : BaseEntity
    {
        
        public string TokenHash { get; set; } = null!;

        public string JwtId { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public string? CreatedByIp { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedByIp { get; set; }

        public Guid? ReplacedByTokenId { get; set; }

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked => RevokedAt != null;

        public bool IsValid => !IsRevoked && !IsExpired;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public RefreshToken() { }

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

        public void Revoke(string? ipAddress = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = ipAddress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReplaceWith(Guid newTokenId)
        {
            ReplacedByTokenId = newTokenId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}