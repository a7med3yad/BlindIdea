using BlindIdea.Application.Dtos.Ideas;
using BlindIdea.Domain.Abstraction;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Encryption;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Application.Services.Implementation.Ideas
{
    public class IdeaService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EncryptionService _encryption;

        public IdeaService(
           IUnitOfWork uow,
           UserManager<ApplicationUser> userManager,
           EncryptionService encryption)
        {
            _uow = uow;
            _userManager = userManager;
            _encryption = encryption;
        }

        public async Task<IdeaResponseDto> SubmitIdeaAsync(string userId, SubmitIdeaDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (user.TeamId == null)
                throw new Exception("You must be in a team to submit ideas");

            var idea = new Idea
            {
                EncryptedTitle = _encryption.Encrypt(dto.Title),       
                EncryptedContent = _encryption.Encrypt(dto.Content),   
                TeamId = user.TeamId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Ideas.AddAsync(idea);
            await _uow.SaveChangesAsync();

            return MapToDto(idea, null);
        }

        public async Task<IEnumerable<IdeaResponseDto>> GetTeamIdeasAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (user.TeamId == null)
                throw new Exception("You must be in a team to view ideas");

            var ideas = await _uow.Ideas.GetTeamIdeaAsync(user.TeamId);

            return ideas.Select(idea =>
            {
                // ✅ Find current user's rating for this idea
                var myRating = idea.Ratings
                    .FirstOrDefault(r => r.UserId == userId)?.Score;

                return MapToDto(idea, myRating);
            });
        }

        public async Task<IdeaResponseDto> GetIdeaAsync(string userId, string ideaId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (user.TeamId == null)
                throw new Exception("You must be in a team to view ideas");

            var idea = await _uow.Ideas.GetIdeaWithRatingsAsync(ideaId)
                ?? throw new Exception("Idea not found");

            // ✅ Make sure idea belongs to user's team
            if (idea.TeamId != user.TeamId)
                throw new Exception("Access denied — idea belongs to a different team");

            var myRating = idea.Ratings
                .FirstOrDefault(r => r.UserId == userId)?.Score;

            return MapToDto(idea, myRating);
        }

        public async Task DeleteIdeaAsync(string userId, string ideaId)
        {
            var idea = await _uow.Ideas.GetbyIdAsync(ideaId)
                ?? throw new Exception("Idea not found");

            // ✅ Only author can delete their own idea
            if (idea.UserId != userId)
                throw new Exception("You can only delete your own ideas");

            _uow.Ideas.Delete(idea);
            await _uow.SaveChangesAsync();
        }

        public async Task<IdeaResponseDto> RateIdeaAsync(string userId, string ideaId, RateIdeaDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (user.TeamId == null)
                throw new Exception("You must be in a team to rate ideas");

            var idea = await _uow.Ideas.GetIdeaWithRatingsAsync(ideaId)
                ?? throw new Exception("Idea not found");

            // ✅ Make sure idea belongs to user's team
            if (idea.TeamId != user.TeamId)
                throw new Exception("Access denied — idea belongs to a different team");

            // ✅ Cannot rate your own idea
            if (idea.UserId == userId)
                throw new Exception("You cannot rate your own idea");

            // ✅ Check if already rated
            var existingRating = await _uow.Ratings.GetUserRatingAsync(userId, ideaId);

            if (existingRating != null)
            {
                // ✅ Update existing rating
                existingRating.Score = dto.Score;
                _uow.Ratings.Update(existingRating);
            }
            else
            {
                // ✅ Create new rating
                var rating = new Rating
                {
                    Score = dto.Score,
                    IdeaId = ideaId,
                    UserId = userId
                };
                await _uow.Ratings.AddAsync(rating);
            }

            await _uow.SaveChangesAsync();

            // ✅ Reload idea with updated ratings
            idea = await _uow.Ideas.GetIdeaWithRatingsAsync(ideaId)!;

            return MapToDto(idea!, dto.Score);
        }

        public async Task DeleteRatingAsync(string userId, string ideaId)
        {
            var rating = await _uow.Ratings.GetUserRatingAsync(userId, ideaId)
                ?? throw new Exception("Rating not found");

            _uow.Ratings.Delete(rating);
            await _uow.SaveChangesAsync();
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
