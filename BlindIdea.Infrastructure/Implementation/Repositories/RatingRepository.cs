using BlindIdea.Domain.Abstraction;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Infrastructure.Implementation.Repositories
{
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        public RatingRepository(AppDbContext context):base(context) { }
        public async Task<Rating?> GetUserRatingAsync(string userId, string ideaId)
            => await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.IdeaId == ideaId);
        public async Task<double> GetAverageRatingAsync(string ideaId)
        {
            var rating = await _context.Ratings
                .Where(r => r.IdeaId == ideaId)
                .ToListAsync();
            return rating.Any() ? rating.Average(r => r.Score) : 0;
        }
        public async Task<IEnumerable<Rating>> GetIdeaRatingsAsync(string ideaId)
            => await _context.Ratings
            .Where(r => r.IdeaId == ideaId)
            .ToListAsync();

        
    }
}
