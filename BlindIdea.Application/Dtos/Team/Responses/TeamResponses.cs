using System;

namespace BlindIdea.Application.Dtos.Team.Responses
{
    /// <summary>
    /// Complete team information response.
    /// Includes all team details, members, and ideas.
    /// </summary>
    public class TeamResponse
    {
        /// <summary>
        /// Unique team identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Team name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Team description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Team admin information.
        /// </summary>
        public UserBasicResponse Admin { get; set; } = null!;

        /// <summary>
        /// Number of team members.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Number of ideas posted to team.
        /// </summary>
        public int IdeaCount { get; set; }

        /// <summary>
        /// When the team was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the team was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Brief team information for list views.
    /// Includes only essential information.
    /// </summary>
    public class TeamSummaryResponse
    {
        /// <summary>
        /// Unique team identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Team name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Team admin ID.
        /// </summary>
        public string AdminId { get; set; } = null!;

        /// <summary>
        /// Number of team members.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Number of ideas in team.
        /// </summary>
        public int IdeaCount { get; set; }
    }

    /// <summary>
    /// Team members list response.
    /// </summary>
    public class TeamMembersResponse
    {
        /// <summary>
        /// Unique team identifier.
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// Team name.
        /// </summary>
        public string TeamName { get; set; } = null!;

        /// <summary>
        /// List of team members.
        /// </summary>
        public List<TeamMemberResponse> Members { get; set; } = new();

        /// <summary>
        /// Total number of members.
        /// </summary>
        public int TotalMembers { get; set; }
    }

    /// <summary>
    /// Individual team member information.
    /// </summary>
    public class TeamMemberResponse
    {
        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// User's display name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Whether this user is the team admin.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// When the user joined the team.
        /// </summary>
        public DateTime JoinedAt { get; set; }
    }

    /// <summary>
    /// Basic user information for inclusion in other responses.
    /// </summary>
    public class UserBasicResponse
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User's display name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = null!;
    }
}
