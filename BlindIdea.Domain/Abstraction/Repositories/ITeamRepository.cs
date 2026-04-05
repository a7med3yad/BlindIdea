using BlindIdea.Domain.Entities;

namespace BlindIdea.Domain.Abstraction.Repositories
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<Team?> GetByInviteCodeAsync(string inviteCode);
        Task<Team?> GetTeamWithMembersAsync(string teamId);
        Task<Team?> GetTeamWithIdeaAsync(string teamId);
        Task<bool> InviteCodeExistsAsync(string inviteCode);

        Task<IEnumerable<Team>> GetUserTeamsAsync(string userId);
        Task<UserTeam?> GetUserTeamAsync(string userId, string teamId);
        Task<bool> IsUserInTeamAsync(string userId, string teamId);
    }
}