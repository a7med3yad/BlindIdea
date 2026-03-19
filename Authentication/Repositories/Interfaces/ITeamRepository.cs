using BlindIdea.API.Entities;

namespace BlindIdea.API.Repositories.Interfaces
{
    public interface ITeamRepository:IGenericRepository<Team>
    {
        Task<Team?> GetByInviteCodeAsync(string inviteCode);
        Task<Team?> GetTeamWithMembersAsync(string teamId);
        Task<Team?> GetTeamWithIdeaAsync(string teamId);
        Task<bool> InviteCodeExistsAsync(string inviteCode);
    }
}
