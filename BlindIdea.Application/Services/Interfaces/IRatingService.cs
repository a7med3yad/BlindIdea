using BlindIdea.Application.Dtos.Rating.Requests;
using BlindIdea.Application.Dtos.Rating.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    
    public interface IRatingService
    {
        
        Task<RatingResponse?> CreateRatingAsync(CreateRatingRequest request, string userId);

        Task<RatingResponse?> GetRatingAsync(Guid ratingId, string? userId = null);

        Task<RatingListResponse> GetIdeaRatingsAsync(ListRatingsRequest request);

        Task<RatingResponse?> GetUserRatingAsync(Guid ideaId, string userId);

        Task<RatingResponse?> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, string userId);

        Task<bool> DeleteRatingAsync(Guid ratingId, string userId);

        Task<bool> CanUserEditAsync(Guid ratingId, string userId);

        Task<bool> CanUserDeleteAsync(Guid ratingId, string userId);

        Task<(bool isValid, string? errorMessage)> ValidateRatingAsync(Guid ideaId, string userId);

        Task<RatingStatisticsResponse?> GetRatingStatisticsAsync(Guid ideaId);

        Task<RatingListResponse> GetUserRatingsAsync(string userId, int pageNumber = 1, int pageSize = 10);

        Task<int> GetRatingCountAsync(Guid ideaId);

        Task<double> GetAverageRatingAsync(Guid ideaId);
    }
}