using BlindIdea.Application.Dtos.Teams;

namespace BlindIdea.Application.Services.Abstraction.Teams
{
    public interface ITeamService
    {

        Task<IEnumerable<TeamResponseDto>> GetMyTeamsAsync(string userId);
        Task<TeamResponseDto> GetActiveTeamAsync(string userId);
        Task<List<TeamMemberDto>> GetMembersAsync(string userId);
        Task<TeamResponseDto> CreateTeamAsync(string userId, CreateTeamDto dto);
        Task<TeamResponseDto> JoinTeamAsync(string userId, JoinTeamDto dto);
        Task LeaveTeamAsync(string userId, string teamId);
        Task DeleteTeamAsync(string userId, string teamId);
        Task<string> RegenerateInviteCodeAsync(string userId, string teamId);
        Task RemoveMemberAsync(string adminId, string memberId, string teamId);
        Task<TeamResponseDto> SwitchTeamAsync(string userId, SwitchTeamDto dto);
    }
}