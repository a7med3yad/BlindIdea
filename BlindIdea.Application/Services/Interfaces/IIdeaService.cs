using BlindIdea.Application.Dtos.Idea.Requests;
using BlindIdea.Application.Dtos.Idea.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    
    public interface IIdeaService
    {
        
        Task<IdeaResponse?> CreateIdeaAsync(CreateIdeaRequest request, string userId);

        Task<IdeaResponse?> GetIdeaAsync(Guid ideaId, string? userId = null);

        Task<IdeaListResponse> SearchIdeasAsync(SearchIdeasRequest request);

        Task<IdeaListResponse> GetTeamIdeasAsync(Guid teamId, int pageNumber = 1, int pageSize = 10);

        Task<IdeaListResponse> GetUserIdeasAsync(string userId, int pageNumber = 1, int pageSize = 10);

        Task<IdeaResponse?> UpdateIdeaAsync(Guid ideaId, UpdateIdeaRequest request, string userId);

        Task<bool> DeleteIdeaAsync(Guid ideaId, string userId);

        Task<bool> CanUserEditAsync(Guid ideaId, string userId);

        Task<bool> CanUserDeleteAsync(Guid ideaId, string userId);

        Task<bool> CanUserRateAsync(Guid ideaId, string userId);

        Task<IdeaStatisticsResponse> GetStatisticsAsync();

        Task<List<IdeaSummaryResponse>> GetTopIdeasAsync(int count = 10);

        Task<List<IdeaSummaryResponse>> GetRecentIdeasAsync(int count = 10);
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
}