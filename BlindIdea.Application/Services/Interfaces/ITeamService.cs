using BlindIdea.Application.Dtos.Team.Requests;
using BlindIdea.Application.Dtos.Team.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Service for managing teams and team operations.
    /// Handles team creation, membership, and team-level authorization.
    /// </summary>
    public interface ITeamService
    {
        // ===== TEAM CRUD =====

        /// <summary>
        /// Creates a new team.
        /// Current user becomes the team admin.
        /// </summary>
        /// <param name="request">Team creation details</param>
        /// <param name="userId">User ID of the creator (becomes admin)</param>
        /// <returns>Created team information, or null if creation fails</returns>
        Task<TeamResponse?> CreateTeamAsync(CreateTeamRequest request, string userId);

        /// <summary>
        /// Gets team by ID.
        /// </summary>
        /// <param name="teamId">Team ID to retrieve</param>
        /// <returns>Team information, or null if not found</returns>
        Task<TeamResponse?> GetTeamAsync(Guid teamId);

        /// <summary>
        /// Gets all teams (with optional filtering).
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of teams per page</param>
        /// <returns>Paginated list of teams</returns>
        Task<(List<TeamSummaryResponse> teams, int totalCount)> GetTeamsAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets teams where user is a member.
        /// </summary>
        /// <param name="userId">User ID to find teams for</param>
        /// <returns>List of teams user belongs to</returns>
        Task<List<TeamResponse>> GetUserTeamsAsync(string userId);

        /// <summary>
        /// Updates team information.
        /// Only team admin can update.
        /// </summary>
        /// <param name="teamId">Team ID to update</param>
        /// <param name="request">Updated team details</param>
        /// <param name="userId">User ID performing update (must be admin)</param>
        /// <returns>Updated team, or null if not found or unauthorized</returns>
        Task<TeamResponse?> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, string userId);

        /// <summary>
        /// Deletes a team.
        /// Only team admin can delete.
        /// Soft-deletes the team (marks as deleted, data remains).
        /// </summary>
        /// <param name="teamId">Team ID to delete</param>
        /// <param name="userId">User ID performing delete (must be admin)</param>
        /// <returns>True if deleted, false if not found or unauthorized</returns>
        Task<bool> DeleteTeamAsync(Guid teamId, string userId);

        // ===== TEAM MEMBERSHIP =====

        /// <summary>
        /// Adds a user to a team.
        /// Only team admin can add members.
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="request">Contains user ID to add</param>
        /// <param name="adminId">User ID of requester (must be admin)</param>
        /// <returns>Updated team members, or null if unauthorized</returns>
        Task<TeamMembersResponse?> AddMemberAsync(Guid teamId, AddTeamMemberRequest request, string adminId);

        /// <summary>
        /// Removes a user from a team.
        /// Admin can remove others; users can remove themselves.
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="request">Contains user ID to remove</param>
        /// <param name="requesterId">User ID of requester</param>
        /// <returns>True if removed, false if unauthorized or not found</returns>
        Task<bool> RemoveMemberAsync(Guid teamId, RemoveTeamMemberRequest request, string requesterId);

        /// <summary>
        /// Gets all members of a team.
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <returns>List of team members</returns>
        Task<TeamMembersResponse?> GetTeamMembersAsync(Guid teamId);

        /// <summary>
        /// Checks if a user is a member of a team.
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user is member, false otherwise</returns>
        Task<bool> IsTeamMemberAsync(Guid teamId, string userId);

        /// <summary>
        /// Checks if a user is the admin of a team.
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user is admin, false otherwise</returns>
        Task<bool> IsTeamAdminAsync(Guid teamId, string userId);

        // ===== ADMIN TRANSFER =====

        /// <summary>
        /// Transfers team admin role to another user.
        /// Only current admin can transfer.
        /// New admin must be a team member.
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <param name="request">Contains new admin user ID</param>
        /// <param name="currentAdminId">User ID of current admin</param>
        /// <returns>Updated team, or null if unauthorized or invalid</returns>
        Task<TeamResponse?> TransferAdminAsync(Guid teamId, TransferAdminRequest request, string currentAdminId);

        // ===== SEARCH & FILTER =====

        /// <summary>
        /// Searches teams by name.
        /// </summary>
        /// <param name="searchTerm">Search term to match in team name</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <returns>Paginated search results</returns>
        Task<(List<TeamSummaryResponse> teams, int totalCount)> SearchTeamsAsync(
            string searchTerm, int pageNumber = 1, int pageSize = 10);

        // ===== STATISTICS =====

        /// <summary>
        /// Gets team statistics (member count, idea count, rating info).
        /// </summary>
        /// <param name="teamId">Team ID</param>
        /// <returns>Team statistics, or null if not found</returns>
        Task<TeamStatisticsResponse?> GetTeamStatisticsAsync(Guid teamId);
    }

    /// <summary>
    /// Team statistics response.
    /// </summary>
    public class TeamStatisticsResponse
    {
        /// <summary>
        /// Team ID.
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// Number of team members.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Number of ideas posted to team.
        /// </summary>
        public int IdeaCount { get; set; }

        /// <summary>
        /// Total ratings received on team ideas.
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Average rating across all team ideas.
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Date team was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
