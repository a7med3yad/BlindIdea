using BlindIdea.API.Core;
using BlindIdea.API.Repositories;
using BlindIdea.API.Repositories.Interfaces;

namespace BlindIdea.API.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ITeamRepository Teams { get; }
        public IIdeaRepository Ideas { get; }
        public IRatingRepository Ratings { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Teams = new TeamRepository(context);
            Ideas = new IdeaRepository(context);
            Ratings = new RatingRepository(context);
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}