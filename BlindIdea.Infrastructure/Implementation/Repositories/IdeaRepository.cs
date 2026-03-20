using BlindIdea.Domain.Abstraction;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Infrastructure.Implementation.Repositories
{
    public class IdeaRepository : GenericRepository<Idea>, IIdeaRepository
    {
        public IdeaRepository(AppDbContext context):base(context) { }

        public async Task<IEnumerable<Idea>> GetTeamIdeaAsync(string teamId)
            => await _context.Ideas
            .Include(i=>i.Ratings)
            .Where(i=>i.TeamId== teamId)
            .OrderByDescending(i=>i.CreatedAt)
            .ToListAsync();
        public async Task<Idea?> GetIdeaWithRatingsAsync(string ideaId)
            =>await _context.Ideas
            .Include(i=>i.Ratings)
            .FirstOrDefaultAsync(i=>i.Id == ideaId);

        public async Task<IEnumerable<Idea>> GetTopRatedIdeasAsync(string teamId, int count = 5)
            =>await _context.Ideas
            .Include(i=>i.Ratings)
            .Where(i=>i.TeamId==teamId && i.Ratings.Any())
            .OrderByDescending(i=>i.Ratings.Average(r=>r.Score))
            .Take(count)
            .ToListAsync();
    }
}
