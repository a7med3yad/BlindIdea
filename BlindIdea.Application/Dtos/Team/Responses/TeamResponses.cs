using BlindIdea.Application.Dtos.Common;

namespace BlindIdea.Application.Dtos.Team.Responses;

public class TeamResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public UserBasicResponse Admin { get; set; } = null!;
    public int MemberCount { get; set; }
    public int IdeaCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TeamSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string AdminId { get; set; } = null!;
    public int MemberCount { get; set; }
    public int IdeaCount { get; set; }
}

public class TeamMembersResponse
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public List<TeamMemberResponse> Members { get; set; } = new();
    public int TotalMembers { get; set; }
}

public class TeamMemberResponse
{
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class TeamStatisticsResponse
{
    public Guid TeamId { get; set; }
    public int MemberCount { get; set; }
    public int IdeaCount { get; set; }
    public int TotalRatings { get; set; }
    public double AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
}