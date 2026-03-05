using BlindIdea.Application.Dtos.Idea.Requests;
using BlindIdea.Application.Dtos.Idea.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Service for managing ideas and idea-related operations.
    /// Handles idea CRUD, searching, filtering, and authorization.
    /// </summary>
    public interface IIdeaService
    {
        // ===== IDEA CRUD =====

        /// <summary>
        /// Creates a new idea.
        /// Ideas can be personal or posted to a team.
        /// </summary>
        /// <param name="request">Idea creation details</param>
        /// <param name="userId">User ID of the idea creator</param>
        /// <returns>Created idea information, or null if creation fails</returns>
        Task<IdeaResponse?> CreateIdeaAsync(CreateIdeaRequest request, string userId);

        /// <summary>
        /// Gets a specific idea by ID.
        /// Includes rating and creator information.
        /// </summary>
        /// <param name="ideaId">Idea ID to retrieve</param>
        /// <param name="userId">Optional user ID to check permissions</param>
        /// <returns>Idea information, or null if not found</returns>
        Task<IdeaResponse?> GetIdeaAsync(Guid ideaId, string? userId = null);

        /// <summary>
        /// Gets all ideas with filtering and sorting.
        /// </summary>
        /// <param name="request">Search and filtering criteria</param>
        /// <returns>Paginated list of ideas</returns>
        Task<IdeaListResponse> SearchIdeasAsync(SearchIdeasRequest request);

        /// <summary>
        /// Gets ideas for a specific team.
        /// </summary>
        /// <param name="teamId">Team ID to get ideas for</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of ideas per page</param>
        /// <returns>Paginated team ideas</returns>
        Task<IdeaListResponse> GetTeamIdeasAsync(Guid teamId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets ideas created by a specific user.
        /// </summary>
        /// <param name="userId">User ID to get ideas for</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of ideas per page</param>
        /// <returns>Paginated user ideas</returns>
        Task<IdeaListResponse> GetUserIdeasAsync(string userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Updates an existing idea.
        /// Only creator or admin can update.
        /// </summary>
        /// <param name="ideaId">Idea ID to update</param>
        /// <param name="request">Updated idea details</param>
        /// <param name="userId">User ID performing update (must be creator or admin)</param>
        /// <returns>Updated idea, or null if not found or unauthorized</returns>
        Task<IdeaResponse?> UpdateIdeaAsync(Guid ideaId, UpdateIdeaRequest request, string userId);

        /// <summary>
        /// Deletes an idea.
        /// Only creator or admin can delete.
        /// Soft-deletes the idea (marks as deleted).
        /// </summary>
        /// <param name="ideaId">Idea ID to delete</param>
        /// <param name="userId">User ID performing delete (must be creator or admin)</param>
        /// <returns>True if deleted, false if not found or unauthorized</returns>
        Task<bool> DeleteIdeaAsync(Guid ideaId, string userId);

        // ===== PERMISSIONS =====

        /// <summary>
        /// Checks if a user can edit an idea.
        /// Only creator and admins can edit.
        /// </summary>
        /// <param name="ideaId">Idea ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user can edit, false otherwise</returns>
        Task<bool> CanUserEditAsync(Guid ideaId, string userId);

        /// <summary>
        /// Checks if a user can delete an idea.
        /// Only creator and admins can delete.
        /// </summary>
        /// <param name="ideaId">Idea ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user can delete, false otherwise</returns>
        Task<bool> CanUserDeleteAsync(Guid ideaId, string userId);

        /// <summary>
        /// Checks if a user can rate an idea.
        /// Users cannot rate their own ideas.
        /// </summary>
        /// <param name="ideaId">Idea ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user can rate, false otherwise</returns>
        Task<bool> CanUserRateAsync(Guid ideaId, string userId);

        // ===== STATISTICS =====

        /// <summary>
        /// Gets aggregate statistics for all ideas.
        /// </summary>
        /// <returns>Statistics object</returns>
        Task<IdeaStatisticsResponse> GetStatisticsAsync();

        /// <summary>
        /// Gets top-rated ideas.
        /// </summary>
        /// <param name="count">Number of ideas to return</param>
        /// <returns>List of top ideas by average rating</returns>
        Task<List<IdeaSummaryResponse>> GetTopIdeasAsync(int count = 10);

        /// <summary>
        /// Gets recently created ideas.
        /// </summary>
        /// <param name="count">Number of ideas to return</param>
        /// <returns>List of recent ideas</returns>
        Task<List<IdeaSummaryResponse>> GetRecentIdeasAsync(int count = 10);
    }

    /// <summary>
    /// Idea statistics response.
    /// </summary>
    public class IdeaStatisticsResponse
    {
        /// <summary>
        /// Total number of ideas.
        /// </summary>
        public int TotalIdeas { get; set; }

        /// <summary>
        /// Total number of ratings.
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Average rating across all ideas.
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Number of anonymous ideas.
        /// </summary>
        public int AnonymousIdeas { get; set; }

        /// <summary>
        /// Number of team ideas.
        /// </summary>
        public int TeamIdeas { get; set; }

        /// <summary>
        /// Number of personal ideas.
        /// </summary>
        public int PersonalIdeas { get; set; }
    }
}
