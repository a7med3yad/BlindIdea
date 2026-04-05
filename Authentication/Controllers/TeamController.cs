using BlindIdea.Application.Common;
using BlindIdea.Application.Dtos.Teams;
using BlindIdea.Application.Services.Abstraction.Teams;
using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Abstraction.UnitOfWorks;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Cache;
using Microsoft.AspNetCore.Identity;

namespace BlindIdea.Application.Services.Implementation.Teams
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICacheService _cache;

        public TeamService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            ICacheService cache)
        {
            _uow = uow;
            _userManager = userManager;
            _cache = cache;
        }

        #region === TEAM LIST & ACTIVE TEAM ===

        public async Task<IEnumerable<TeamResponseDto>> GetMyTeamsAsync(string userId)
        {
            var userTeams = await _uow.UserTeams.GetUserTeamsWithTeamsAsync(userId);
            return userTeams.Select(ut => MapToDto(ut.Team, ut));
        }

        public async Task<TeamResponseDto> GetActiveTeamAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You have no active team");

            var userTeam = await _uow.UserTeams.GetUserTeamAsync(userId, user.ActiveTeamId)
                ?? throw new Exception("Active team record not found");

            var team = await _uow.Teams.GetTeamWithMembersAsync(user.ActiveTeamId)
                ?? throw new Exception("Team not found");

            return MapToDto(team, userTeam);
        }

        #endregion

        #region === CREATE & JOIN ===

        public async Task<TeamResponseDto> CreateTeamAsync(string userId, CreateTeamDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            var team = new Team
            {
                Name = dto.Name,
                AdminId = userId,
                InviteCode = await GenerateUniqueInviteCode(),
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Teams.AddAsync(team);

            var userTeam = new UserTeam
            {
                UserId = userId,
                TeamId = team.Id,
                IsAdmin = true,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            await _uow.UserTeams.AddAsync(userTeam);

            user.ActiveTeamId = team.Id;
            await _userManager.UpdateAsync(user);

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.AddToRoleAsync(user, "Admin");

            await _uow.SaveChangesAsync();

            _cache.Remove(CacheKeys.UserTeams(userId));

            return MapToDto(team, userTeam);
        }

        public async Task<TeamResponseDto> JoinTeamAsync(string userId, JoinTeamDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            var team = await _uow.Teams.GetByInviteCodeAsync(dto.InviteCode)
                ?? throw new Exception("Invalid invite code");

            if (await _uow.UserTeams.IsUserInTeamAsync(userId, team.Id))
                throw new Exception("You are already a member of this team");

            var userTeam = new UserTeam
            {
                UserId = userId,
                TeamId = team.Id,
                IsAdmin = false,
                IsActive = user.ActiveTeamId == null,
                JoinedAt = DateTime.UtcNow
            };

            await _uow.UserTeams.AddAsync(userTeam);

            if (user.ActiveTeamId == null)
            {
                user.ActiveTeamId = team.Id;
                await _userManager.UpdateAsync(user);
            }

            await _uow.SaveChangesAsync();

            _cache.Remove(CacheKeys.UserTeams(userId));

            return MapToDto(team, userTeam);
        }

        #endregion

        #region === TEAM ACTIONS (with backward compatibility) ===

        // Overloads without teamId → use Active Team
        public async Task LeaveTeamAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You are not in any team");

            await LeaveTeamAsync(userId, user.ActiveTeamId);
        }

        public async Task LeaveTeamAsync(string userId, string teamId)
        {
            var userTeam = await _uow.UserTeams.GetUserTeamAsync(userId, teamId)
                ?? throw new Exception("You are not in this team");

            var team = await _uow.Teams.GetbyIdAsync(teamId)
                ?? throw new Exception("Team not found");

            if (team.AdminId == userId)
                throw new Exception("Admin cannot leave team. Delete or transfer ownership.");

            _uow.UserTeams.Delete(userTeam);

            var user = await _userManager.FindByIdAsync(userId)!;

            if (user.ActiveTeamId == teamId)
            {
                var other = (await _uow.UserTeams.GetUserTeamsWithTeamsAsync(userId))
                    .FirstOrDefault(ut => ut.TeamId != teamId);

                user.ActiveTeamId = other?.TeamId;
                if (other != null) other.IsActive = true;
            }

            await _userManager.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            _cache.Remove(CacheKeys.UserTeams(userId));
        }

        public async Task DeleteTeamAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You are not in any team");

            await DeleteTeamAsync(userId, user.ActiveTeamId);
        }

        public async Task DeleteTeamAsync(string userId, string teamId)
        { /* ... same as before ... */
            // (Copy the full DeleteTeamAsync with teamId from previous version)
            var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("User not found");
            var team = await _uow.Teams.GetTeamWithMembersAsync(teamId) ?? throw new Exception("Team not found");

            if (team.AdminId != userId)
                throw new Exception("Only admin can delete the team");

            var teamIdToClear = team.Id;
            var userTeamsList = team.UserTeams.ToList();

            foreach (var ut in userTeamsList)
            {
                if (ut.User.ActiveTeamId == teamIdToClear)
                    ut.User.ActiveTeamId = null;
                await _userManager.UpdateAsync(ut.User);
                _uow.UserTeams.Delete(ut);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.RemoveFromRoleAsync(user, "Admin");

            _uow.Teams.Delete(team);
            await _uow.SaveChangesAsync();

            _cache.RemoveMany(
                CacheKeys.Team(teamIdToClear),
                CacheKeys.TeamMembers(teamIdToClear),
                CacheKeys.Dashboard(teamIdToClear),
                CacheKeys.TeamIdeas(teamIdToClear),
                CacheKeys.TopIdeas(teamIdToClear),
                CacheKeys.UserTeams(userId)
            );
        }

        public async Task<string> RegenerateInviteCodeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You are not in any team");

            return await RegenerateInviteCodeAsync(userId, user.ActiveTeamId);
        }

        public async Task<string> RegenerateInviteCodeAsync(string userId, string teamId)
        {
            var team = await _uow.Teams.GetbyIdAsync(teamId)
                ?? throw new Exception("Team not found");

            if (team.AdminId != userId)
                throw new Exception("Only admin can regenerate invite code");

            team.InviteCode = await GenerateUniqueInviteCode();
            _uow.Teams.Update(team);

            await _uow.SaveChangesAsync();
            _cache.Remove(CacheKeys.Team(teamId));

            return team.InviteCode;
        }

        public async Task RemoveMemberAsync(string adminId, string memberId)
        {
            var admin = await _userManager.FindByIdAsync(adminId)
                ?? throw new Exception("Admin not found");

            if (string.IsNullOrEmpty(admin.ActiveTeamId))
                throw new Exception("You are not in any team");

            await RemoveMemberAsync(adminId, memberId, admin.ActiveTeamId);
        }

        public async Task RemoveMemberAsync(string adminId, string memberId, string teamId)
        {
            var team = await _uow.Teams.GetbyIdAsync(teamId)
                ?? throw new Exception("Team not found");

            if (team.AdminId != adminId)
                throw new Exception("Only admin can remove members");

            if (adminId == memberId)
                throw new Exception("Admin cannot remove themselves");

            var userTeam = await _uow.UserTeams.GetUserTeamAsync(memberId, teamId)
                ?? throw new Exception("User is not in this team");

            _uow.UserTeams.Delete(userTeam);

            var member = await _userManager.FindByIdAsync(memberId)!;
            if (member.ActiveTeamId == teamId)
                member.ActiveTeamId = null;

            await _userManager.UpdateAsync(member);
            await _uow.SaveChangesAsync();

            _cache.RemoveMany(
                CacheKeys.Team(teamId),
                CacheKeys.TeamMembers(teamId),
                CacheKeys.Dashboard(teamId),
                CacheKeys.UserTeams(memberId)
            );
        }

        #endregion

        #region === MEMBERS & SWITCH ===

        public async Task<List<TeamMemberDto>> GetMembersAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (string.IsNullOrEmpty(user.ActiveTeamId))
                throw new Exception("You are not in any team");

            var teamId = user.ActiveTeamId;

            return await _cache.GetOrSetAsync(
                CacheKeys.TeamMembers(teamId),
                async () =>
                {
                    var userTeams = await _uow.UserTeams.GetTeamMembersAsync(teamId);
                    return userTeams.Select(ut => new TeamMemberDto
                    {
                        Id = ut.User.Id,
                        Email = ut.User.Email!,
                        IsAdmin = ut.IsAdmin
                    }).ToList();
                },
                CacheDurations.TeamMembers
            );
        }

        public async Task<TeamResponseDto> SwitchTeamAsync(string userId, SwitchTeamDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            if (!await _uow.UserTeams.IsUserInTeamAsync(userId, dto.TeamId))
                throw new Exception("You are not a member of this team");

            var allUserTeams = await _uow.UserTeams.GetUserTeamsWithTeamsAsync(userId);

            foreach (var ut in allUserTeams)
                ut.IsActive = false;

            var targetUserTeam = allUserTeams.First(ut => ut.TeamId == dto.TeamId);
            targetUserTeam.IsActive = true;
            user.ActiveTeamId = dto.TeamId;

            await _userManager.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            _cache.Remove(CacheKeys.UserTeams(userId));

            return MapToDto(targetUserTeam.Team, targetUserTeam);
        }

        #endregion

        #region === HELPERS ===

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

        private TeamResponseDto MapToDto(Team team, UserTeam userTeam) => new()
        {
            Id = team.Id,
            Name = team.Name,
            InviteCode = team.InviteCode,
            AdminId = team.AdminId,
            MemberCount = team.UserTeams.Count,
            CreatedAt = team.CreatedAt,
            IsAdmin = userTeam.IsAdmin,
            IsActive = userTeam.IsActive,
            JoinedAt = userTeam.JoinedAt
        };

        #endregion
    }
}