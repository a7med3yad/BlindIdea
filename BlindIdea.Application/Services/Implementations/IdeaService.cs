using BlindIdea.Application.Dtos;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Application.Services.Implementations
{
    public class IdeaService : IIdeaService
    {
        private readonly AppDbContext _context;

        public IdeaService(AppDbContext context)
        {
            _context = context;
        }

        // ================= Create Idea =================
        public async Task<Guid> CreateAsync(CreateIdeaDto dto, string userId)
        {
            // Using Object Initializer (Entities have private setters)
            var idea = new Idea
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                IsAnonymous = dto.IsAnonymous,
                UserId = userId,
                TeamId = dto.TeamId
            };

            _context.Ideas.Add(idea);
            await _context.SaveChangesAsync();

            return idea.Id;
        }

        // ================= Get Idea by Id =================
        public async Task<IdeaDto?> GetByIdAsync(Guid id)
        {
            var idea = await _context.Ideas
                .Include(i => i.Ratings)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (idea == null)
                return null;

            return new IdeaDto
            {
                Id = idea.Id,
                Title = idea.Title,
                Description = idea.Description,
                IsAnonymous = idea.IsAnonymous,
                UserId = idea.UserId,
                TeamId = idea.TeamId,
                AverageRating = idea.Ratings.Any()
                    ? idea.Ratings.Average(r => r.Value)
                    : 0
            };
        }

        // ================= Get All Ideas =================
        public async Task<List<IdeaDto>> GetAllAsync()
        {
            return await _context.Ideas
                .Include(i => i.Ratings)
                .Select(i => new IdeaDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    IsAnonymous = i.IsAnonymous,
                    UserId = i.UserId,
                    TeamId = i.TeamId,
                    AverageRating = i.Ratings.Any()
                        ? i.Ratings.Average(r => r.Value)
                        : 0
                })
                .ToListAsync();
        }

        // ================= Rate Idea =================
        public async Task RateAsync(Guid ideaId, int value, string userId)
        {
            var alreadyRated = await _context.Ratings
                .AnyAsync(r => r.IdeaId == ideaId && r.UserId == userId);

            if (alreadyRated)
                throw new Exception("You already rated this idea.");

            // Using Object Initializer for Rating
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                IdeaId = ideaId,
                UserId = userId,
                Value = value
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
        }
    }
}
