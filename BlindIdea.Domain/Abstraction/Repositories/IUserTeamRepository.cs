using BlindIdea.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Domain.Abstraction.Repositories
{
    public interface IUserTeamRepository:IGenericRepository<UserTeam>
    {
        Task<UserTeam?> GetUserTeamAsync(string userId, string teamId);
        Task<bool> IsUserInTeamAsync(string userId, string teamId);
        Task<IEnumerable<UserTeam>> GetUserTeamsWithTeamsAsync(string userId);
        Task<IEnumerable<UserTeam>> GetTeamMembersAsync(string teamId);
    }
}
