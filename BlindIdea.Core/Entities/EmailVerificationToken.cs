using System;

namespace BlindIdea.Core.Entities
{
    
    public class EmailVerificationToken : BaseEntity
    {
        
        public string TokenHash { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public bool IsUsed => VerifiedAt != null;

        public bool IsValid => !IsExpired && !IsUsed;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public EmailVerificationToken() { }

        public EmailVerificationToken(string userId, string tokenHash, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsVerified()
        {
            VerifiedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool ValidateForUse()
        {
            return IsValid && !IsExpired && !IsUsed;
        }
    }
}