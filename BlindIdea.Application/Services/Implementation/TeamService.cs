using BlindIdea.Application.Dtos.Teams.Requests;
using BlindIdea.Application.Dtos.Teams.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Application.Services.Implementations;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public TeamService(IUnitOfWork unitOfWork, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<TeamResponse> CreateTeamAsync(string adminId, CreateTeamRequest request, CancellationToken ct = default)
    {
        var admin = await _userManager.FindByIdAsync(adminId)
            ?? throw new InvalidOperationException("User not found.");

        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            AdminId = adminId,
            IsDeleted = false
        };

        // Add the admin as the first member
        var members = (List<User>)team.Members;
        members.Add(admin);

        await _unitOfWork.Teams.AddAsync(team);
        await _unitOfWork.CommitAsync(ct);

        return MapToTeamResponse(team, adminId);
    }

    public async Task<TeamResponse> GetTeamAsync(int teamId, string requestingUserId, CancellationToken ct = default)
    {
        var team = await GetActiveTeamAsync(teamId);
        EnsureIsMember(team, requestingUserId);
        return MapToTeamResponse(team, requestingUserId);
    }

    public async Task<IEnumerable<TeamResponse>> GetUserTeamsAsync(string userId, CancellationToken ct = default)
    {
        var allTeams = await _unitOfWork.Teams.FindAsync(t => t.IsDeleted == false);
        var userTeams = allTeams.Where(t =>
            t.AdminId == userId || t.Members.Any(m => m.Id == userId));

        return userTeams.Select(t => MapToTeamResponse(t, userId));
    }

    public async Task<TeamResponse> UpdateTeamAsync(int teamId, string adminId, UpdateTeamRequest request, CancellationToken ct = default)
    {
        var team = await GetActiveTeamAsync(teamId);
        EnsureIsAdmin(team, adminId);

        if (request.Name != null) team.Name = request.Name;
        if (request.Description != null) team.Description = request.Description;

        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync(ct);

        return MapToTeamResponse(team, adminId);
    }

    public async Task DeleteTeamAsync(int teamId, string adminId, CancellationToken ct = default)
    {
        var team = await GetActiveTeamAsync(teamId);
        EnsureIsAdmin(team, adminId);

        team.IsDeleted = true;
        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync(ct);
    }

    public async Task AddMemberAsync(int teamId, string adminId, AddMemberRequest request, CancellationToken ct = default)
    {
        var team = await GetActiveTeamAsync(teamId);
        EnsureIsAdmin(team, adminId);

        var newMember = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User to add not found.");

        if (newMember.IsDeleted)
            throw new InvalidOperationException("Cannot add a deleted user.");

        if (team.Members.Any(m => m.Id == request.UserId))
            throw new InvalidOperationException("User is already a member of this team.");

        var members = (List<User>)team.Members;
        members.Add(newMember);

        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync(ct);
    }

    public async Task RemoveMemberAsync(int teamId, string adminId, string memberId, CancellationToken ct = default)
    {
        var team = await GetActiveTeamAsync(teamId);
        EnsureIsAdmin(team, adminId);

        if (memberId == adminId)
            throw new InvalidOperationException("Admin cannot remove themselves from the team.");

        var member = team.Members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new InvalidOperationException("User is not a member of this team.");

        var members = (List<User>)team.Members;
        members.Remove(member);

        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync(ct);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<Team> GetActiveTeamAsync(int teamId)
    {
        var teams = await _unitOfWork.Teams.FindAsync(t => t.Id == teamId && t.IsDeleted == false);
        return teams.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");
    }

    private static void EnsureIsAdmin(Team team, string userId)
    {
        if (team.AdminId != userId)
            throw new UnauthorizedAccessException("Only the team admin can perform this action.");
    }

    private static void EnsureIsMember(Team team, string userId)
    {
        if (!team.Members.Any(m => m.Id == userId) && team.AdminId != userId)
            throw new UnauthorizedAccessException("You are not a member of this team.");
    }

    private static TeamResponse MapToTeamResponse(Team team, string requestingUserId) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Description = team.Description,
        AdminId = team.AdminId,
        AdminName = team.Admin?.FullName ?? string.Empty,
        IsAdmin = team.AdminId == requestingUserId,
        IsMember = team.Members.Any(m => m.Id == requestingUserId),
        MemberCount = team.Members.Count,
        Members = team.Members.Select(m => new TeamMemberResponse
        {
            UserId = m.Id,
            UserName = m.UserName ?? string.Empty,
            FullName = m.FullName,
            Email = m.Email ?? string.Empty
        }).ToList()
    };
}
