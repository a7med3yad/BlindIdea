namespace BlindIdea.Core.Entities;

public class TeamMember
{
    public Guid TeamId { get; set; }
    public virtual Team Team { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}
