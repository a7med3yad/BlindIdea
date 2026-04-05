using BlindIdea.Domain.Abstraction.Repositories;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Infrastructure.Implementation.Repositories
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(AppDbContext context) : base(context) { }

        public async Task<Team?> GetByInviteCodeAsync(string inviteCode)
            => await _context.Teams
             .Include(t => t.Members)
             .FirstOrDefaultAsync(t=>t.InviteCode == inviteCode);

        public async Task<Team?> GetTeamWithMembersAsync(string teamId)
            => await _context.Teams
             .Include(t => t.Members)
             .FirstOrDefaultAsync(t => t.Id == teamId);

        public async Task<Team?> GetTeamWithIdeaAsync(string teamId)
            => await _context.Teams
            .Include(t => t.Ideas)
                .ThenInclude(i => i.Ratings)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        public async Task<bool> InviteCodeExistsAsync(string inviteCode)
            => await _context.Teams.AnyAsync(t => t.InviteCode == inviteCode);

        public async Task<IEnumerable<Team>> GetUserTeamsAsync(string userId)
            => await _context.UserTeams
            .AsNoTracking()
            .Where(ut => ut.UserId == userId)
            .Include(ut => ut.Team)
                .ThenInclude(t => t.UserTeams)
            .Select(ut => ut.Team)
            .ToListAsync();

        public async Task<UserTeam?> GetUserTeamAsync(string userId, string teamId)
            => await _context.UserTeams
                .FirstOrDefaultAsync(ut =>
                    ut.UserId == userId && ut.TeamId == teamId);

        public async Task<bool> IsUserInTeamAsync(string userId, string teamId)
            => await _context.UserTeams
                .AnyAsync(ut => ut.UserId == userId && ut.TeamId == teamId);

       
    }
}
