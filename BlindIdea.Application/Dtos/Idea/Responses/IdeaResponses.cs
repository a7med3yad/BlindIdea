using BlindIdea.Application.Dtos.Common;
using System;

namespace BlindIdea.Application.Dtos.Idea.Responses;
    
    public class IdeaResponse
    {
        
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsAnonymous { get; set; }

        public UserBasicResponse? Creator { get; set; }

        public TeamBasicResponse? Team { get; set; }

        public double AverageRating { get; set; }

        public int RatingCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDelete { get; set; }

        public int? CurrentUserRating { get; set; }
    }

    public class IdeaSummaryResponse
    {
        
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsAnonymous { get; set; }

        public string CreatorName { get; set; } = null!;

        public double AverageRating { get; set; }

        public int RatingCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class IdeaListResponse
    {
        
        public List<IdeaSummaryResponse> Ideas { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasNextPage => PageNumber < TotalPages;

        public bool HasPreviousPage => PageNumber > 1;
    }

    public class TeamBasicResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class IdeaStatisticsResponse
    {
        public int TotalIdeas { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public int AnonymousIdeas { get; set; }
        public int TeamIdeas { get; set; }
        public int PersonalIdeas { get; set; }
    }