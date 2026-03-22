using BlindIdea.Application.Common;
using BlindIdea.Application.Dtos;
using BlindIdea.Application.Services.Abstraction.Dashboards;
using BlindIdea.Domain.Abstraction.UnitOfWorks;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Cache;
using BlindIdea.Infrastructure.Implementation.Encryption;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Application.Services.Implementation.Dashboards
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EncryptionService _encryption;
        private readonly CacheService _cache; 

        public DashboardService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            EncryptionService encryption,
            CacheService cache) 
        {
            _uow = uow;
            _userManager = userManager;
            _encryption = encryption;
            _cache = cache;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (user.TeamId == null)
                throw new Exception("You must be in a team to view dashboard");

            return await _cache.GetOrSetAsync(
                CacheKeys.Dashboard(user.TeamId),
                () => BuildDashboardAsync(user.TeamId, userId),
                CacheDurations.Dashboard
            );
        }

        private async Task<DashboardResponseDto> BuildDashboardAsync(
            string teamId, string userId)
        {
            var team = await _uow.Teams.GetTeamWithMembersAsync(teamId)
                ?? throw new Exception("Team not found");

            var ideas = (await _uow.Ideas.GetTeamIdeaAsync(teamId)).ToList();
            var allRatings = ideas.SelectMany(i => i.Ratings).ToList();

            return new DashboardResponseDto
            {
                Team = GetTeamSummary(team),
                Ideas = GetIdeaSummary(ideas, allRatings, userId),
                TopIdeas = GetTopIdeas(ideas),
                RecentIdeas = GetRecentIdeas(ideas)
            };
        }

        private TeamSummaryDto GetTeamSummary(Team team) => new()
        {
            TeamName = team.Name,
            MemberCount = team.Members.Count,
            CreatedAt = team.CreatedAt
        };

        private IdeaSummaryDto GetIdeaSummary(
            List<Idea> ideas,
            List<Rating> allRatings,
            string userId) => new()
            {
                TotalIdeas = ideas.Count,
                TotalRatings = allRatings.Count,
                OverallAverageRating = allRatings.Any()
                ? Math.Round(allRatings.Average(r => r.Score), 1)
                : 0,
                IdeasSubmittedByMe = ideas.Count(i => i.UserId == userId),
                IdeasRatedByMe = allRatings.Count(r => r.UserId == userId)
            };

        private List<TopIdeaDto> GetTopIdeas(List<Idea> ideas) =>
            ideas
                .Where(i => i.Ratings.Any())
                .OrderByDescending(i => i.Ratings.Average(r => r.Score))
                .Take(5)
                .Select(i => new TopIdeaDto
                {
                    Id = i.Id,
                    Title = _encryption.Decrypt(i.EncryptedTitle),
                    AverageRating = Math.Round(
                        i.Ratings.Average(r => r.Score), 1),
                    RatingCount = i.Ratings.Count,
                    CreatedAt = i.CreatedAt
                })
                .ToList();

        private List<RecentIdeaDto> GetRecentIdeas(List<Idea> ideas) =>
            ideas
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => new RecentIdeaDto
                {
                    Id = i.Id,
                    Title = _encryption.Decrypt(i.EncryptedTitle),
                    AverageRating = i.Ratings.Any()
                        ? Math.Round(i.Ratings.Average(r => r.Score), 1)
                        : 0,
                    RatingCount = i.Ratings.Count,
                    CreatedAt = i.CreatedAt
                })
                .ToList();
    }
}
