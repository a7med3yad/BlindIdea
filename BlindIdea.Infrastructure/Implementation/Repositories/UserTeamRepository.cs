using BlindIdea.Domain.Abstraction.Repositories;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Implementation.Repositories
{
    public class UserTeamRepository : GenericRepository<UserTeam>, IUserTeamRepository
    {
        public UserTeamRepository(AppDbContext context) : base(context) { }

        public async Task<UserTeam?> GetUserTeamAsync(string userId, string teamId)
            => await _dbSet
                .Include(ut => ut.Team)
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TeamId == teamId);

        public async Task<bool> IsUserInTeamAsync(string userId, string teamId)
            => await _dbSet.AnyAsync(ut => ut.UserId == userId && ut.TeamId == teamId);

        public async Task<IEnumerable<UserTeam>> GetUserTeamsWithTeamsAsync(string userId)
            => await _dbSet
                .Include(ut => ut.Team).ThenInclude(t => t.UserTeams).Where(ut => ut.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<UserTeam>> GetTeamMembersAsync(string teamId)
            => await _dbSet
                .Include(ut => ut.User)
                .Where(ut => ut.TeamId == teamId)
                .ToListAsync();
    }
}

