using BlindIdea.Application.Common;
using BlindIdea.Application.Dtos.Ideas;
using BlindIdea.Application.Services.Abstraction.Ideas;
using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Abstraction.UnitOfWorks;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Cache;
using BlindIdea.Infrastructure.Implementation.Encryption;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Application.Services.Implementation.Ideas
{
    public class IdeaService : IIdeaService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEncryptionService _encryption;
        private readonly ICacheService _cache;

        public IdeaService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            IEncryptionService encryption,
            ICacheService cache)
        {
            _uow = uow;
            _userManager = userManager;
            _encryption = encryption;
            _cache = cache;
        }

        public async Task<IEnumerable<IdeaResponseDto>> GetTeamIdeasAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You must be in a team to view ideas");

            var activeTeamId = user.ActiveTeamId;

            var ideas = await _cache.GetOrSetAsync(
                CacheKeys.TeamIdeas(activeTeamId),
                () => _uow.Ideas.GetTeamIdeaAsync(activeTeamId),
                CacheDurations.Ideas
            );

            return ideas.Select(idea =>
            {
                var myRating = idea.Ratings
                    .FirstOrDefault(r => r.UserId == userId)?.Score;
                return MapToDto(idea, myRating);
            });
        }

        public async Task<IdeaResponseDto> SubmitIdeaAsync(string userId, SubmitIdeaDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You must be in a team to submit ideas");

            var activeTeamId = user.ActiveTeamId;

            var idea = new Idea
            {
                EncryptedTitle = _encryption.Encrypt(dto.Title),
                EncryptedContent = _encryption.Encrypt(dto.Content),
                TeamId = activeTeamId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Ideas.AddAsync(idea);
            await _uow.SaveChangesAsync();

            _cache.RemoveMany(
                CacheKeys.TeamIdeas(activeTeamId),
                CacheKeys.Dashboard(activeTeamId),
                CacheKeys.TopIdeas(activeTeamId)
            );

            return MapToDto(idea, null);
        }

        public async Task DeleteIdeaAsync(string userId, string ideaId)
        {
            var idea = await _uow.Ideas.GetbyIdAsync(ideaId)
                ?? throw new Exception("Idea not found");

            if (idea.UserId != userId)
                throw new Exception("You can only delete your own ideas");

            var teamId = idea.TeamId;

            _uow.Ideas.Delete(idea);
            await _uow.SaveChangesAsync();

            _cache.RemoveMany(
                CacheKeys.TeamIdeas(teamId),
                CacheKeys.Dashboard(teamId),
                CacheKeys.TopIdeas(teamId)
            );
        }

        public async Task<IdeaResponseDto> RateIdeaAsync(
            string userId, string ideaId, RateIdeaDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You must be in a team to rate ideas");

            var idea = await _uow.Ideas.GetIdeaWithRatingsAsync(ideaId)
                ?? throw new Exception("Idea not found");

            if (idea.TeamId != user.ActiveTeamId)
                throw new Exception("Access denied");

            if (idea.UserId == userId)
                throw new Exception("You cannot rate your own idea");

            var existingRating = await _uow.Ratings.GetUserRatingAsync(userId, ideaId);

            if (existingRating != null)
            {
                existingRating.Score = dto.Score;
                _uow.Ratings.Update(existingRating);
            }
            else
            {
                var rating = new Rating
                {
                    Score = dto.Score,
                    IdeaId = ideaId,
                    UserId = userId
                };
                await _uow.Ratings.AddAsync(rating);
            }

            await _uow.SaveChangesAsync();

            _cache.RemoveMany(
                CacheKeys.Dashboard(user.ActiveTeamId),
                CacheKeys.TopIdeas(user.ActiveTeamId),
                CacheKeys.TeamIdeas(user.ActiveTeamId)
            );

            idea = await _uow.Ideas.GetIdeaWithRatingsAsync(ideaId)!;
            return MapToDto(idea!, dto.Score);
        }

        public async Task DeleteRatingAsync(string userId, string ideaId)
        {
            var rating = await _uow.Ratings.GetUserRatingAsync(userId, ideaId)
                ?? throw new Exception("Rating not found");

            var idea = await _uow.Ideas.GetbyIdAsync(ideaId);
            var teamId = idea?.TeamId;

            _uow.Ratings.Delete(rating);
            await _uow.SaveChangesAsync();

            if (teamId != null)
            {
                _cache.RemoveMany(
                    CacheKeys.Dashboard(teamId),
                    CacheKeys.TopIdeas(teamId),
                    CacheKeys.TeamIdeas(teamId)
                );
            }
        }

        public async Task<IdeaResponseDto> GetIdeaAsync(string userId, string ideaId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You must be in a team to view ideas");

            var idea = await _uow.Ideas.GetIdeaWithRatingsAsync(ideaId)
                ?? throw new Exception("Idea not found");

            if (idea.TeamId != user.ActiveTeamId)
                throw new Exception("Access denied");

            var myRating = idea.Ratings
                .FirstOrDefault(r => r.UserId == userId)?.Score;

            return MapToDto(idea, myRating);
        }

        private IdeaResponseDto MapToDto(Idea idea, int? myRating) => new()
        {
            Id = idea.Id,
            Title = _encryption.Decrypt(idea.EncryptedTitle),
            Content = _encryption.Decrypt(idea.EncryptedContent),
            CreatedAt = idea.CreatedAt,
            AverageRating = idea.Ratings.Any()
                ? Math.Round(idea.Ratings.Average(r => r.Score), 1)
                : 0,
            RatingCount = idea.Ratings.Count,
            MyRating = myRating
        };
    }
}