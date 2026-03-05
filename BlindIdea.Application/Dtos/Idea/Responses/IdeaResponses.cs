using System;

namespace BlindIdea.Application.Dtos.Idea.Responses
{
    /// <summary>
    /// Complete idea information response.
    /// Includes all details, ratings, and creator information.
    /// </summary>
    public class IdeaResponse
    {
        /// <summary>
        /// Unique idea identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Idea title.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Idea description.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Whether the idea was posted anonymously.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Creator information (null if anonymous).
        /// </summary>
        public UserBasicResponse? Creator { get; set; }

        /// <summary>
        /// Team information if posted to a team.
        /// </summary>
        public TeamBasicResponse? Team { get; set; }

        /// <summary>
        /// Average rating of the idea (0-5).
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of ratings received.
        /// </summary>
        public int RatingCount { get; set; }

        /// <summary>
        /// When the idea was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the idea was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Whether the current user can edit this idea.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Whether the current user can delete this idea.
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Current user's rating if they have rated this (1-5), or null.
        /// </summary>
        public int? CurrentUserRating { get; set; }
    }

    /// <summary>
    /// Brief idea information for list views.
    /// </summary>
    public class IdeaSummaryResponse
    {
        /// <summary>
        /// Unique idea identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Idea title.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Short description (truncated if longer).
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Is idea anonymous.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Creator's name or "Anonymous".
        /// </summary>
        public string CreatorName { get; set; } = null!;

        /// <summary>
        /// Average rating.
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Number of ratings.
        /// </summary>
        public int RatingCount { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Ideas paginated list response.
    /// </summary>
    public class IdeaListResponse
    {
        /// <summary>
        /// List of ideas on this page.
        /// </summary>
        public List<IdeaSummaryResponse> Ideas { get; set; } = new();

        /// <summary>
        /// Current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of ideas.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Whether there's a next page.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Whether there's a previous page.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// Basic team information for inclusion in other responses.
    /// </summary>
    public class TeamBasicResponse
    {
        /// <summary>
        /// Team ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Team name.
        /// </summary>
        public string Name { get; set; } = null!;
    }

    /// <summary>
    /// Basic user information for inclusion in other responses.
    /// </summary>
    public class UserBasicResponse
    {
        /// <summary>
        /// User ID.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User's display name.
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
