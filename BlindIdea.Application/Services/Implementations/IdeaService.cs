using BlindIdea.Application.Dtos.Idea.Requests;
using BlindIdea.Application.Dtos.Idea.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Implementations
{
    public class IdeaService : IIdeaService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<IdeaService> _logger;

        public IdeaService(UserManager<User> userManager, ILogger<IdeaService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IdeaResponse?> CreateIdeaAsync(CreateIdeaRequest request, string userId) => null;
        public async Task<IdeaResponse?> GetIdeaAsync(Guid ideaId, string? userId = null) => null;
        public async Task<IdeaListResponse> SearchIdeasAsync(SearchIdeasRequest request) => new IdeaListResponse();
        public async Task<IdeaListResponse> GetTeamIdeasAsync(Guid teamId, int pageNumber = 1, int pageSize = 10) => new IdeaListResponse();
        public async Task<IdeaListResponse> GetUserIdeasAsync(string userId, int pageNumber = 1, int pageSize = 10) => new IdeaListResponse();
        public async Task<IdeaResponse?> UpdateIdeaAsync(Guid ideaId, UpdateIdeaRequest request, string userId) => null;
        public async Task<bool> DeleteIdeaAsync(Guid ideaId, string userId) => false;
        public async Task<bool> CanUserEditAsync(Guid ideaId, string userId) => false;
        public async Task<bool> CanUserDeleteAsync(Guid ideaId, string userId) => false;
        public async Task<bool> CanUserRateAsync(Guid ideaId, string userId) => false;
        public async Task<IdeaStatisticsResponse> GetStatisticsAsync() => new IdeaStatisticsResponse();
        public async Task<List<IdeaSummaryResponse>> GetTopIdeasAsync(int count = 10) => new();
        public async Task<List<IdeaSummaryResponse>> GetRecentIdeasAsync(int count = 10) => new();
    }
}
