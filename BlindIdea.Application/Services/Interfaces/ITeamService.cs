using BlindIdea.Application.Dtos.Team.Requests;
using BlindIdea.Application.Dtos.Team.Responses;

namespace BlindIdea.Application.Services.Interfaces;

public interface ITeamService
{
    Task<TeamResponse?> CreateTeamAsync(CreateTeamRequest request, string userId);
    Task<TeamResponse?> GetTeamAsync(Guid teamId);
    Task<(List<TeamSummaryResponse> teams, int totalCount)> GetTeamsAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<TeamResponse>> GetUserTeamsAsync(string userId);
    Task<TeamResponse?> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, string userId);
    Task<bool> DeleteTeamAsync(Guid teamId, string userId);

    // Member management
    Task<TeamMembersResponse?> AddMemberAsync(Guid teamId, AddTeamMemberRequest request, string adminId);
    Task<bool> RemoveMemberAsync(Guid teamId, string userIdToRemove, string requesterId);
    Task<TeamMembersResponse?> JoinTeamAsync(Guid teamId, string userId);
    Task<bool> LeaveTeamAsync(Guid teamId, string userId);
    Task<TeamMembersResponse?> GetTeamMembersAsync(Guid teamId);
    Task<bool> IsTeamMemberAsync(Guid teamId, string userId);
    Task<bool> IsTeamAdminAsync(Guid teamId, string userId);
    Task<TeamResponse?> TransferAdminAsync(Guid teamId, TransferAdminRequest request, string currentAdminId);

    // Search & stats
    Task<(List<TeamSummaryResponse> teams, int totalCount)> SearchTeamsAsync(
        string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<TeamStatisticsResponse?> GetTeamStatisticsAsync(Guid teamId);
}