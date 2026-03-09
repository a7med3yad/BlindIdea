namespace BlindIdea.Core.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
}
