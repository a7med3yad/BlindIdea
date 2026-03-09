namespace BlindIdea.Core.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = null!;
    public string AdminId { get; set; } = null!;
    public virtual User Admin { get; set; } = null!;
    public string? Description { get; set; }

    public Team() { }

    public Team(string name, string adminId, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        AdminId = adminId;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private readonly List<TeamMember> _members = new();
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    private readonly List<Idea> _ideas = new();
    public IReadOnlyCollection<Idea> Ideas => _ideas.AsReadOnly();

    public void AddMember(TeamMember member)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (_members.Any(m => m.UserId == member.UserId)) return;
        _members.Add(member);
    }

    public void RemoveMember(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null) _members.Remove(member);
    }

    public void AddIdea(Idea idea)
    {
        if (idea == null) throw new ArgumentNullException(nameof(idea));
        if (_ideas.Any(i => i.Id == idea.Id)) return;
        _ideas.Add(idea);
    }

    public void RemoveIdea(Guid ideaId)
    {
        var idea = _ideas.FirstOrDefault(i => i.Id == ideaId);
        if (idea != null) _ideas.Remove(idea);
    }

    public bool IsAdmin(string userId) => AdminId == userId;

    public bool IsMember(string userId) => _members.Any(m => m.UserId == userId);
}
