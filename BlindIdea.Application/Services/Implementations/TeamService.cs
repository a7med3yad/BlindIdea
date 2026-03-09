using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.Team.Requests;
using BlindIdea.Application.Dtos.Team.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Application.Services.Implementations;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeamResponse?> CreateTeamAsync(CreateTeamRequest request, string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User must be authenticated");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var existingTeam = await _unitOfWork.Teams.FirstOrDefaultAsync(t => t.Name == request.Name);
        if (existingTeam != null)
            throw new InvalidOperationException("Team with this name already exists");

        var team = new Team(request.Name, userId, request.Description);

        // Add the creator as the first member (admin)
        var adminMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.Teams.AddAsync(team);
        await _unitOfWork.TeamMembers.AddAsync(adminMember);
        await _unitOfWork.CommitAsync();

        return MapToResponse(team, user, 1, 0);
    }

    public async Task<TeamResponse?> GetTeamAsync(Guid teamId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null) return null;

        var admin = await _unitOfWork.Users.GetByIdAsync(team.AdminId);
        var memberCount = await _unitOfWork.TeamMembers.CountAsync(tm => tm.TeamId == teamId);
        var ideaCount = await _unitOfWork.Ideas.CountAsync(i => i.TeamId == teamId);

        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Admin = admin != null ? new UserBasicResponse { Id = admin.Id!, Name = admin.FullName, Email = admin.Email } : null!,
            MemberCount = memberCount,
            IdeaCount = ideaCount,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }

    public async Task<(List<TeamSummaryResponse> teams, int totalCount)> GetTeamsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var query = _unitOfWork.Teams.AsQueryable().Where(t => !t.IsDeleted);
        var totalCount = await query.CountAsync();
        var teams = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var teamIds = teams.Select(t => t.Id).ToList();

        // Batch fetch member/idea counts
        var memberCounts = await _unitOfWork.TeamMembers.AsQueryable()
            .Where(tm => teamIds.Contains(tm.TeamId))
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count);

        var ideaCounts = await _unitOfWork.Ideas.AsQueryable()
            .Where(i => i.TeamId.HasValue && teamIds.Contains(i.TeamId.Value))
            .GroupBy(i => i.TeamId!.Value)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count);

        var result = teams.Select(t => new TeamSummaryResponse
        {
            Id = t.Id,
            Name = t.Name,
            AdminId = t.AdminId,
            MemberCount = memberCounts.GetValueOrDefault(t.Id, 0),
            IdeaCount = ideaCounts.GetValueOrDefault(t.Id, 0)
        }).ToList();

        return (result, totalCount);
    }

    public async Task<List<TeamResponse>> GetUserTeamsAsync(string userId)
    {
        var memberShips = await _unitOfWork.TeamMembers.FindAsync(tm => tm.UserId == userId);
        var teamIds = memberShips.Select(tm => tm.TeamId).Distinct().ToList();

        if (!teamIds.Any()) return new List<TeamResponse>();

        var teams = await _unitOfWork.Teams.AsQueryable()
            .Where(t => teamIds.Contains(t.Id) && !t.IsDeleted)
            .ToListAsync();

        var result = new List<TeamResponse>();
        foreach (var team in teams)
        {
            var admin = await _unitOfWork.Users.GetByIdAsync(team.AdminId);
            var memberCount = await _unitOfWork.TeamMembers.CountAsync(tm => tm.TeamId == team.Id);
            var ideaCount = await _unitOfWork.Ideas.CountAsync(i => i.TeamId == team.Id);
            result.Add(new TeamResponse
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Admin = admin != null ? new UserBasicResponse { Id = admin.Id!, Name = admin.FullName, Email = admin.Email } : null!,
                MemberCount = memberCount,
                IdeaCount = ideaCount,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            });
        }

        return result;
    }

    public async Task<TeamResponse?> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, string userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null) return null;
        if (team.AdminId != userId)
            throw new UnauthorizedAccessException("Only team admin can update team");

        team.Name = request.Name;
        team.Description = request.Description;
        team.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync();

        var admin = await _unitOfWork.Users.GetByIdAsync(team.AdminId);
        var memberCount = await _unitOfWork.TeamMembers.CountAsync(tm => tm.TeamId == teamId);
        var ideaCount = await _unitOfWork.Ideas.CountAsync(i => i.TeamId == teamId);

        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Admin = admin != null ? new UserBasicResponse { Id = admin.Id!, Name = admin.FullName, Email = admin.Email } : null!,
            MemberCount = memberCount,
            IdeaCount = ideaCount,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }

    public async Task<bool> DeleteTeamAsync(Guid teamId, string userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null) return false;
        if (team.AdminId != userId)
            throw new UnauthorizedAccessException("Only team admin can delete team");

        team.IsDeleted = true;
        team.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task<TeamMembersResponse?> JoinTeamAsync(Guid teamId, string userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException("Team not found");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var existing = await _unitOfWork.TeamMembers.FirstOrDefaultAsync(
            tm => tm.TeamId == teamId && tm.UserId == userId);
        if (existing != null)
            throw new InvalidOperationException("You are already a member of this team");

        var member = new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
        await _unitOfWork.TeamMembers.AddAsync(member);
        await _unitOfWork.CommitAsync();

        return await GetTeamMembersAsync(teamId);
    }

    public async Task<bool> LeaveTeamAsync(Guid teamId, string userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException("Team not found");

        if (team.AdminId == userId)
            throw new InvalidOperationException("Team admin cannot leave the team. Transfer admin role first.");

        var member = await _unitOfWork.TeamMembers.FirstOrDefaultAsync(
            tm => tm.TeamId == teamId && tm.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("You are not a member of this team");

        _unitOfWork.TeamMembers.Delete(member);
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task<TeamMembersResponse?> AddMemberAsync(Guid teamId, AddTeamMemberRequest request, string adminId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException("Team not found");
        if (team.AdminId != adminId)
            throw new UnauthorizedAccessException("Only team admin can add members");

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var existing = await _unitOfWork.TeamMembers.FirstOrDefaultAsync(
            tm => tm.TeamId == teamId && tm.UserId == request.UserId);
        if (existing != null)
            throw new InvalidOperationException("User is already a team member");

        var member = new TeamMember
        {
            TeamId = teamId,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow
        };
        await _unitOfWork.TeamMembers.AddAsync(member);
        await _unitOfWork.CommitAsync();

        return await GetTeamMembersAsync(teamId);
    }

    public async Task<bool> RemoveMemberAsync(Guid teamId, string userIdToRemove, string requesterId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException("Team not found");

        var isAdmin = team.AdminId == requesterId;
        var isSelf = userIdToRemove == requesterId;

        if (!isAdmin && !isSelf)
            throw new UnauthorizedAccessException("Only admin or the member themselves can remove a member");

        if (team.AdminId == userIdToRemove)
            throw new InvalidOperationException("Cannot remove team admin. Transfer admin role first.");

        var member = await _unitOfWork.TeamMembers.FirstOrDefaultAsync(
            tm => tm.TeamId == teamId && tm.UserId == userIdToRemove);
        if (member == null)
            throw new KeyNotFoundException("Member not found in this team");

        _unitOfWork.TeamMembers.Delete(member);
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task<TeamMembersResponse?> GetTeamMembersAsync(Guid teamId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null) return null;

        var members = await _unitOfWork.TeamMembers.FindAsync(tm => tm.TeamId == teamId);
        var userIds = members.Select(m => m.UserId).Distinct().ToList();
        var users = (await _unitOfWork.Users.GetAllAsync())
            .Where(u => userIds.Contains(u.Id!))
            .ToDictionary(u => u.Id!);

        var memberResponses = members.Select(m => new TeamMemberResponse
        {
            UserId = m.UserId,
            Name = users.GetValueOrDefault(m.UserId)?.FullName ?? "Unknown",
            Email = users.GetValueOrDefault(m.UserId)?.Email ?? "",
            IsAdmin = m.UserId == team.AdminId,
            JoinedAt = m.JoinedAt
        }).OrderByDescending(m => m.IsAdmin).ThenBy(m => m.JoinedAt).ToList();

        return new TeamMembersResponse
        {
            TeamId = teamId,
            TeamName = team.Name,
            Members = memberResponses,
            TotalMembers = memberResponses.Count
        };
    }

    public async Task<bool> IsTeamMemberAsync(Guid teamId, string userId) =>
        await _unitOfWork.TeamMembers.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

    public async Task<bool> IsTeamAdminAsync(Guid teamId, string userId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        return team != null && team.AdminId == userId;
    }

    public async Task<TeamResponse?> TransferAdminAsync(Guid teamId, TransferAdminRequest request, string currentAdminId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException("Team not found");
        if (team.AdminId != currentAdminId)
            throw new UnauthorizedAccessException("Only admin can transfer ownership");

        var isMember = await _unitOfWork.TeamMembers.AnyAsync(
            tm => tm.TeamId == teamId && tm.UserId == request.NewAdminId);
        if (!isMember)
            throw new InvalidOperationException("New admin must be a current team member");

        team.AdminId = request.NewAdminId;
        team.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Teams.Update(team);
        await _unitOfWork.CommitAsync();

        var newAdmin = await _unitOfWork.Users.GetByIdAsync(request.NewAdminId);
        var memberCount = await _unitOfWork.TeamMembers.CountAsync(tm => tm.TeamId == teamId);
        var ideaCount = await _unitOfWork.Ideas.CountAsync(i => i.TeamId == teamId);

        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Admin = newAdmin != null ? new UserBasicResponse { Id = newAdmin.Id!, Name = newAdmin.FullName, Email = newAdmin.Email } : null!,
            MemberCount = memberCount,
            IdeaCount = ideaCount,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }

    public async Task<(List<TeamSummaryResponse> teams, int totalCount)> SearchTeamsAsync(
        string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var query = _unitOfWork.Teams.AsQueryable().Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var teams = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var teamIds = teams.Select(t => t.Id).ToList();

        var memberCounts = await _unitOfWork.TeamMembers.AsQueryable()
            .Where(tm => teamIds.Contains(tm.TeamId))
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count);

        var ideaCounts = await _unitOfWork.Ideas.AsQueryable()
            .Where(i => i.TeamId.HasValue && teamIds.Contains(i.TeamId.Value))
            .GroupBy(i => i.TeamId!.Value)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count);

        var result = teams.Select(t => new TeamSummaryResponse
        {
            Id = t.Id,
            Name = t.Name,
            AdminId = t.AdminId,
            MemberCount = memberCounts.GetValueOrDefault(t.Id, 0),
            IdeaCount = ideaCounts.GetValueOrDefault(t.Id, 0)
        }).ToList();

        return (result, totalCount);
    }

    public async Task<TeamStatisticsResponse?> GetTeamStatisticsAsync(Guid teamId)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
        if (team == null) return null;

        var memberCount = await _unitOfWork.TeamMembers.CountAsync(tm => tm.TeamId == teamId);
        var ideas = await _unitOfWork.Ideas.FindAsync(i => i.TeamId == teamId);
        var ideaList = ideas.ToList();
        var ideaIds = ideaList.Select(i => i.Id).ToList();
        var ratings = await _unitOfWork.Ratings.FindAsync(r => ideaIds.Contains(r.IdeaId));
        var ratingList = ratings.ToList();

        return new TeamStatisticsResponse
        {
            TeamId = teamId,
            MemberCount = memberCount,
            IdeaCount = ideaList.Count,
            TotalRatings = ratingList.Count,
            AverageRating = ratingList.Any() ? Math.Round(ratingList.Average(r => r.Value), 2) : 0,
            CreatedAt = team.CreatedAt
        };
    }

    private static TeamResponse MapToResponse(Team team, User admin, int memberCount, int ideaCount) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Description = team.Description,
        Admin = new UserBasicResponse { Id = admin.Id!, Name = admin.FullName, Email = admin.Email },
        MemberCount = memberCount,
        IdeaCount = ideaCount,
        CreatedAt = team.CreatedAt,
        UpdatedAt = team.UpdatedAt
    };
}
