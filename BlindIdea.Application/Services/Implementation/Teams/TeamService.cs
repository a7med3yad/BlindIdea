using BlindIdea.Application.Common;
using BlindIdea.Application.Dtos.Teams;
using BlindIdea.Application.Services.Abstraction.Teams;
using BlindIdea.Domain.Abstraction.UnitOfWorks;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Cache;
using Microsoft.AspNetCore.Identity;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CacheService _cache; 

    public TeamService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        CacheService cache) 
    {
        _uow = uow;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<TeamResponseDto> GetMyTeamAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId == null)
            throw new Exception("You are not in a team");

        return await _cache.GetOrSetAsync(
            CacheKeys.Team(user.TeamId),
            async () =>
            {
                var team = await _uow.Teams
                    .GetTeamWithMembersAsync(user.TeamId)
                    ?? throw new Exception("Team not found");
                return MapToDto(team, team.Members.Count);
            },
            CacheDurations.Team
        );
    }

    public async Task<List<TeamMemberDto>> GetMembersAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId == null)
            throw new Exception("You are not in a team");

        return await _cache.GetOrSetAsync(
            CacheKeys.TeamMembers(user.TeamId),
            async () =>
            {
                var team = await _uow.Teams
                    .GetTeamWithMembersAsync(user.TeamId)
                    ?? throw new Exception("Team not found");

                return team.Members.Select(m => new TeamMemberDto
                {
                    Id = m.Id,
                    Email = m.Email!,
                    IsAdmin = m.Id == team.AdminId
                }).ToList();
            },
            CacheDurations.TeamMembers
        );
    }

    public async Task<TeamResponseDto> CreateTeamAsync(
        string userId, CreateTeamDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId != null)
            throw new Exception("You are already in a team");

        var team = new Team
        {
            Name = dto.Name,
            AdminId = userId,
            InviteCode = await GenerateUniqueInviteCode(),
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Teams.AddAsync(team);
        user.TeamId = team.Id;
        await _userManager.UpdateAsync(user);

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            await _userManager.AddToRoleAsync(user, "Admin");

        await _uow.SaveChangesAsync();

        var dto_result = MapToDto(team, 1);
        _cache.Set(
            CacheKeys.Team(team.Id),
            dto_result,
            CacheDurations.Team
        );

        return dto_result;
    }

    public async Task<TeamResponseDto> JoinTeamAsync(
        string userId, JoinTeamDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId != null)
            throw new Exception("You are already in a team");

        var team = await _uow.Teams.GetByInviteCodeAsync(dto.InviteCode)
            ?? throw new Exception("Invalid invite code");

        user.TeamId = team.Id;
        await _userManager.UpdateAsync(user);

        _cache.RemoveMany(
            CacheKeys.Team(team.Id),
            CacheKeys.TeamMembers(team.Id),
            CacheKeys.Dashboard(team.Id)
        );

        return MapToDto(team, team.Members.Count + 1);
    }

    public async Task LeaveTeamAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId == null)
            throw new Exception("You are not in a team");

        var team = await _uow.Teams.GetbyIdAsync(user.TeamId)
            ?? throw new Exception("Team not found");

        if (team.AdminId == userId)
            throw new Exception("Admin cannot leave");

        var teamId = user.TeamId;
        user.TeamId = null;
        await _userManager.UpdateAsync(user);

        _cache.RemoveMany(
            CacheKeys.Team(teamId),
            CacheKeys.TeamMembers(teamId),
            CacheKeys.Dashboard(teamId)
        );
    }

    public async Task DeleteTeamAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId == null)
            throw new Exception("You are not in a team");

        var team = await _uow.Teams.GetTeamWithMembersAsync(user.TeamId)
            ?? throw new Exception("Team not found");

        if (team.AdminId != userId)
            throw new Exception("Only admin can delete the team");

        var teamId = team.Id;
        var members = team.Members.ToList();

        foreach (var member in members)
        {
            member.TeamId = null;
            await _userManager.UpdateAsync(member);
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
            await _userManager.RemoveFromRoleAsync(user, "Admin");

        _uow.Teams.Delete(team);
        await _uow.SaveChangesAsync();

        _cache.RemoveMany(
            CacheKeys.Team(teamId),
            CacheKeys.TeamMembers(teamId),
            CacheKeys.Dashboard(teamId),
            CacheKeys.TeamIdeas(teamId),
            CacheKeys.TopIdeas(teamId)
        );
    }

    public async Task<string> RegenerateInviteCodeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        if (user.TeamId == null)
            throw new Exception("You are not in a team");

        var team = await _uow.Teams.GetbyIdAsync(user.TeamId)
            ?? throw new Exception("Team not found");

        if (team.AdminId != userId)
            throw new Exception("Only admin can regenerate invite code");

        team.InviteCode = await GenerateUniqueInviteCode();
        _uow.Teams.Update(team);
        await _uow.SaveChangesAsync();

        _cache.Remove(CacheKeys.Team(user.TeamId));

        return team.InviteCode;
    }

    public async Task RemoveMemberAsync(string adminId, string memberId)
    {
        var admin = await _userManager.FindByIdAsync(adminId)
            ?? throw new Exception("Admin not found");

        if (admin.TeamId == null)
            throw new Exception("You are not in a team");

        var team = await _uow.Teams.GetbyIdAsync(admin.TeamId)
            ?? throw new Exception("Team not found");

        if (team.AdminId != adminId)
            throw new Exception("Only admin can remove members");

        if (adminId == memberId)
            throw new Exception("Admin cannot remove themselves");

        var member = await _userManager.FindByIdAsync(memberId)
            ?? throw new Exception("Member not found");

        if (member.TeamId != team.Id)
            throw new Exception("User is not in your team");

        member.TeamId = null;
        await _userManager.UpdateAsync(member);

        _cache.RemoveMany(
            CacheKeys.Team(admin.TeamId),
            CacheKeys.TeamMembers(admin.TeamId),
            CacheKeys.Dashboard(admin.TeamId)
        );
    }

    private async Task<string> GenerateUniqueInviteCode()
    {
        string code;
        do
        {
            code = Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
        while (await _uow.Teams.InviteCodeExistsAsync(code));
        return code;
    }

    private TeamResponseDto MapToDto(Team team, int memberCount) => new()
    {
        Id = team.Id,
        Name = team.Name,
        InviteCode = team.InviteCode,
        AdminId = team.AdminId,
        MemberCount = memberCount,
        CreatedAt = team.CreatedAt
    };
}