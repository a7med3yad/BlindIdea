using BlindIdea.Application.Dtos.Teams;

namespace BlindIdea.Application.Services.Abstraction.Teams
{
    public interface ITeamService
    {
        Task<TeamResponseDto> CreateTeamAsync(string userId, CreateTeamDto dto);
        Task<TeamResponseDto> JoinTeamAsync(string userId, JoinTeamDto dto);
        Task<TeamResponseDto> GetMyTeamAsync(string userId);
        Task<List<TeamMemberDto>> GetMembersAsync(string userId);
        Task LeaveTeamAsync(string userId);
        Task DeleteTeamAsync(string userId);
        Task<string> RegenerateInviteCodeAsync(string userId);
        Task RemoveMemberAsync(string adminId, string memberId);
    }
}