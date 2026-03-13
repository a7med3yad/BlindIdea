using BlindIdea.Application.Dtos.Teams.Requests;
using BlindIdea.Application.Dtos.Teams.Responses;

namespace BlindIdea.Application.Services.Interfaces;

public interface ITeamService
{
    Task<TeamResponse> CreateTeamAsync(string adminId, CreateTeamRequest request, CancellationToken ct = default);
    Task<TeamResponse> GetTeamAsync(int teamId, string requestingUserId, CancellationToken ct = default);
    Task<IEnumerable<TeamResponse>> GetUserTeamsAsync(string userId, CancellationToken ct = default);
    Task<TeamResponse> UpdateTeamAsync(int teamId, string adminId, UpdateTeamRequest request, CancellationToken ct = default);
    Task DeleteTeamAsync(int teamId, string adminId, CancellationToken ct = default);
    Task AddMemberAsync(int teamId, string adminId, AddMemberRequest request, CancellationToken ct = default);
    Task RemoveMemberAsync(int teamId, string adminId, string memberId, CancellationToken ct = default);
}
