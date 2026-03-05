using BlindIdea.Application.Dtos.Team.Requests;
using BlindIdea.Application.Dtos.Team.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    
    public interface ITeamService
    {
        
        Task<TeamResponse?> CreateTeamAsync(CreateTeamRequest request, string userId);

        Task<TeamResponse?> GetTeamAsync(Guid teamId);

        Task<(List<TeamSummaryResponse> teams, int totalCount)> GetTeamsAsync(int pageNumber = 1, int pageSize = 10);

        Task<List<TeamResponse>> GetUserTeamsAsync(string userId);

        Task<TeamResponse?> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, string userId);

        Task<bool> DeleteTeamAsync(Guid teamId, string userId);

        Task<TeamMembersResponse?> AddMemberAsync(Guid teamId, AddTeamMemberRequest request, string adminId);

        Task<bool> RemoveMemberAsync(Guid teamId, RemoveTeamMemberRequest request, string requesterId);

        Task<TeamMembersResponse?> GetTeamMembersAsync(Guid teamId);

        Task<bool> IsTeamMemberAsync(Guid teamId, string userId);

        Task<bool> IsTeamAdminAsync(Guid teamId, string userId);

        Task<TeamResponse?> TransferAdminAsync(Guid teamId, TransferAdminRequest request, string currentAdminId);

        Task<(List<TeamSummaryResponse> teams, int totalCount)> SearchTeamsAsync(
            string searchTerm, int pageNumber = 1, int pageSize = 10);

        Task<TeamStatisticsResponse?> GetTeamStatisticsAsync(Guid teamId);
    }

    public class TeamStatisticsResponse
    {
        
        public Guid TeamId { get; set; }

        public int MemberCount { get; set; }

        public int IdeaCount { get; set; }

        public int TotalRatings { get; set; }

        public double AverageRating { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}