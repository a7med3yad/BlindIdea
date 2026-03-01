using BlindIdea.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Interfaces
{
   
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(string name, string adminId);
        Task AddUserToTeamAsync(Guid teamId, string adminId, string userId);
        Task<List<TeamDto>> GetAllTeamsAsync();
    }
}
