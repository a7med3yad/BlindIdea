using BlindIdea.Application.Dtos.Rating.Requests;
using BlindIdea.Application.Dtos.Rating.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Service for managing idea ratings and rating-related operations.
    /// Handles rating creation, updates, and retrieval with authorization checks.
    /// </summary>
    public interface IRatingService
    {
        // ===== RATING CRUD =====

        /// <summary>
        /// Creates a new rating for an idea.
        /// Users cannot rate their own ideas.
        /// Prevents duplicate ratings (one rating per user per idea).
        /// </summary>
        /// <param name="request">Rating details (idea ID, value 1-5, optional comment)</param>
        /// <param name="userId">User ID providing the rating</param>
        /// <returns>Created rating, or null if creation fails</returns>
        Task<RatingResponse?> CreateRatingAsync(CreateRatingRequest request, string userId);

        /// <summary>
        /// Gets a specific rating by ID.
        /// </summary>
        /// <param name="ratingId">Rating ID to retrieve</param>
        /// <param name="userId">Optional user ID to check permissions</param>
        /// <returns>Rating information, or null if not found</returns>
        Task<RatingResponse?> GetRatingAsync(Guid ratingId, string? userId = null);

        /// <summary>
        /// Gets all ratings for an idea with pagination and filtering.
        /// </summary>
        /// <param name="request">Filtering and pagination criteria</param>
        /// <returns>Paginated list of ratings with statistics</returns>
        Task<RatingListResponse> GetIdeaRatingsAsync(ListRatingsRequest request);

        /// <summary>
        /// Gets user's rating for a specific idea.
        /// </summary>
        /// <param name="ideaId">Idea ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>Rating if user has rated, null otherwise</returns>
        Task<RatingResponse?> GetUserRatingAsync(Guid ideaId, string userId);

        /// <summary>
        /// Updates a user's existing rating.
        /// Only the user who gave the rating can update it.
        /// </summary>
        /// <param name="ratingId">Rating ID to update</param>
        /// <param name="request">Updated rating details</param>
        /// <param name="userId">User ID of the rater (must match rating owner)</param>
        /// <returns>Updated rating, or null if not found or unauthorized</returns>
        Task<RatingResponse?> UpdateRatingAsync(Guid ratingId, UpdateRatingRequest request, string userId);

        /// <summary>
        /// Deletes a rating.
        /// Only the user who gave the rating can delete it (or admin).
        /// </summary>
        /// <param name="ratingId">Rating ID to delete</param>
        /// <param name="userId">User ID performing delete</param>
        /// <returns>True if deleted, false if not found or unauthorized</returns>
        Task<bool> DeleteRatingAsync(Guid ratingId, string userId);

        // ===== AUTHORIZATION =====

        /// <summary>
        /// Checks if a user can edit a rating.
        /// Only the user who gave the rating can edit it.
        /// </summary>
        /// <param name="ratingId">Rating ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user can edit, false otherwise</returns>
        Task<bool> CanUserEditAsync(Guid ratingId, string userId);

        /// <summary>
        /// Checks if a user can delete a rating.
        /// Only the user who gave the rating can delete it.
        /// </summary>
        /// <param name="ratingId">Rating ID</param>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user can delete, false otherwise</returns>
        Task<bool> CanUserDeleteAsync(Guid ratingId, string userId);

        // ===== VALIDATION =====

        /// <summary>
        /// Validates that a user can rate an idea.
        /// Checks: idea exists, user exists, user isn't creator, no duplicate rating.
        /// </summary>
        /// <param name="ideaId">Idea ID to rate</param>
        /// <param name="userId">User ID attempting to rate</param>
        /// <returns>Validation result with error message if invalid</returns>
        Task<(bool isValid, string? errorMessage)> ValidateRatingAsync(Guid ideaId, string userId);

        // ===== STATISTICS =====

        /// <summary>
        /// Gets rating statistics for an idea.
        /// Includes distribution, averages, and counts by star level.
        /// </summary>
        /// <param name="ideaId">Idea ID to get statistics for</param>
        /// <returns>Rating statistics, or null if idea not found</returns>
        Task<RatingStatisticsResponse?> GetRatingStatisticsAsync(Guid ideaId);

        /// <summary>
        /// Gets all ratings given by a user.
        /// </summary>
        /// <param name="userId">User ID to get ratings for</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of ratings per page</param>
        /// <returns>Paginated list of user's ratings</returns>
        Task<RatingListResponse> GetUserRatingsAsync(string userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets count of ratings for an idea.
        /// </summary>
        /// <param name="ideaId">Idea ID</param>
        /// <returns>Total number of ratings for the idea</returns>
        Task<int> GetRatingCountAsync(Guid ideaId);

        /// <summary>
        /// Gets average rating for an idea.
        /// </summary>
        /// <param name="ideaId">Idea ID</param>
        /// <returns>Average rating (0-5), or 0 if no ratings</returns>
        Task<double> GetAverageRatingAsync(Guid ideaId);
    }
}
