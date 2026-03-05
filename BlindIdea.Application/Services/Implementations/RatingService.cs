using BlindIdea.Application.Dtos.Rating.Requests;
using BlindIdea.Application.Dtos.Rating.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Implementations
{
    public class RatingService : IRatingService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RatingService> _logger;

        public RatingService(UserManager<User> userManager, ILogger<RatingService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<RatingResponse?> CreateRatingAsync(CreateRatingRequest request, string userId) => null;
        public async Task<RatingResponse?> GetRatingAsync(Guid ratingId, string? userId = null) => null;
        public async Task<RatingListResponse> GetIdeaRatingsAsync(ListRatingsRequest request) => new RatingListResponse();
        public async Task<RatingResponse?> GetUserRatingAsync(Guid ideaId, string userId) => null;
        public async Task<RatingResponse?> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, string userId) => null;
        public async Task<bool> DeleteRatingAsync(Guid ratingId, string userId) => false;
        public async Task<bool> CanUserEditAsync(Guid ratingId, string userId) => false;
        public async Task<bool> CanUserDeleteAsync(Guid ratingId, string userId) => false;
        public async Task<(bool isValid, string? errorMessage)> ValidateRatingAsync(Guid ideaId, string userId) => (false, "Not implemented");
        public async Task<RatingStatisticsResponse?> GetRatingStatisticsAsync(Guid ideaId) => null;
        public async Task<RatingListResponse> GetUserRatingsAsync(string userId, int pageNumber = 1, int pageSize = 10) => new RatingListResponse();
        public async Task<int> GetRatingCountAsync(Guid ideaId) => 0;
        public async Task<double> GetAverageRatingAsync(Guid ideaId) => 0.0;
    }
}
