using BlindIdea.Application.Dtos.Common;
using System;

namespace BlindIdea.Application.Dtos.Rating.Responses;
    
    public class RatingResponse
    {
        
        public Guid Id { get; set; }

        public Guid IdeaId { get; set; }

        public UserBasicResponse Rater { get; set; } = null!;

        public int Value { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }
    }

    public class RatingSummaryResponse
    {
        
        public Guid Id { get; set; }

        public string RaterName { get; set; } = null!;

        public int Value { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class RatingListResponse
    {
        
        public List<RatingSummaryResponse> Ratings { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasNextPage => PageNumber < TotalPages;

        public bool HasPreviousPage => PageNumber > 1;

        public RatingStatisticsResponse? Statistics { get; set; }
    }

    public class RatingStatisticsResponse
    {
        
        public double AverageRating { get; set; }

        public int TotalRatings { get; set; }

        public Dictionary<int, int> RatingDistribution { get; set; } = new();

        public int OneStar { get; set; }

        public int TwoStars { get; set; }

        public int ThreeStars { get; set; }

        public int FourStars { get; set; }

        public int FiveStars { get; set; }

        public double PercentFiveStars { get; set; }

        public double PercentPositive { get; set; }
    }