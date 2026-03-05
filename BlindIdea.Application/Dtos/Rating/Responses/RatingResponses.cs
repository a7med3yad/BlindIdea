using System;

namespace BlindIdea.Application.Dtos.Rating.Responses
{
    /// <summary>
    /// Complete rating information response.
    /// </summary>
    public class RatingResponse
    {
        /// <summary>
        /// Unique rating identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The idea ID being rated.
        /// </summary>
        public Guid IdeaId { get; set; }

        /// <summary>
        /// The rater's information.
        /// </summary>
        public UserBasicResponse Rater { get; set; } = null!;

        /// <summary>
        /// The rating value (1-5).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Optional comment explaining the rating.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// When the rating was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the rating was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Whether the current user can edit this rating.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Whether the current user can delete this rating.
        /// </summary>
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// Brief rating information for list views.
    /// </summary>
    public class RatingSummaryResponse
    {
        /// <summary>
        /// Rating ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Rater's name.
        /// </summary>
        public string RaterName { get; set; } = null!;

        /// <summary>
        /// Rating value (1-5).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Brief comment preview.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Ratings list response with pagination.
    /// </summary>
    public class RatingListResponse
    {
        /// <summary>
        /// List of ratings on this page.
        /// </summary>
        public List<RatingSummaryResponse> Ratings { get; set; } = new();

        /// <summary>
        /// Current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of ratings per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of ratings.
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

        /// <summary>
        /// Overall statistics for the idea.
        /// </summary>
        public RatingStatisticsResponse Statistics { get; set; } = null!;
    }

    /// <summary>
    /// Rating statistics for an idea.
    /// </summary>
    public class RatingStatisticsResponse
    {
        /// <summary>
        /// Average rating (0-5).
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of ratings.
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Distribution of ratings by stars.
        /// Key is rating value (1-5), Value is count.
        /// </summary>
        public Dictionary<int, int> RatingDistribution { get; set; } = new();

        /// <summary>
        /// Number of 1-star ratings.
        /// </summary>
        public int OneStar { get; set; }

        /// <summary>
        /// Number of 2-star ratings.
        /// </summary>
        public int TwoStars { get; set; }

        /// <summary>
        /// Number of 3-star ratings.
        /// </summary>
        public int ThreeStars { get; set; }

        /// <summary>
        /// Number of 4-star ratings.
        /// </summary>
        public int FourStars { get; set; }

        /// <summary>
        /// Number of 5-star ratings.
        /// </summary>
        public int FiveStars { get; set; }

        /// <summary>
        /// Percentage of ratings that are 5-star.
        /// </summary>
        public double PercentFiveStars { get; set; }

        /// <summary>
        /// Percentage of ratings that are 4-5 stars (positive).
        /// </summary>
        public double PercentPositive { get; set; }
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
