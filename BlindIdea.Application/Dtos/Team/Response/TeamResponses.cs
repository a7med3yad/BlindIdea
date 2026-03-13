namespace BlindIdea.Application.Dtos.Teams.Responses;

public class TeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string AdminId { get; set; } = null!;
    public string AdminName { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public bool IsMember { get; set; }
    public int MemberCount { get; set; }
    public List<TeamMemberResponse> Members { get; set; } = new();
}

public class TeamMemberResponse
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
