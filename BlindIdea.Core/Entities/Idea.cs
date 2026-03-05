using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Idea entity representing a user-submitted idea or suggestion.
    /// Ideas can be posted to teams or by individuals, and receive ratings from other users.
    /// Supports anonymous submission mode.
    /// </summary>
    public class Idea : BaseEntity
    {
        /// <summary>
        /// Idea title - brief summary of the idea.
        /// Required, maximum 200 characters.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed description of the idea and its benefits.
        /// Required, maximum 2000 characters.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Flag indicating if idea is posted anonymously.
        /// When true, the submitter's identity is hidden from viewers.
        /// </summary>
        public bool IsAnonymous { get; set; } = false;

        /// <summary>
        /// Foreign key to the User who submitted the idea.
        /// Even for anonymous ideas, we track the creator for admin purposes.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the User who created the idea.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Foreign key to the Team (optional).
        /// Null if idea is personal/standalone.
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// Navigation property to the Team (if any).
        /// </summary>
        public virtual Team? Team { get; set; }

        /// <summary>
        /// Ratings collection (backing field for internal access).
        /// Use AddRating/RemoveRating to maintain invariants.
        /// </summary>
        private readonly List<Rating> _ratings = new();

        /// <summary>
        /// Read-only collection of ratings for this idea.
        /// </summary>
        public IReadOnlyCollection<Rating> Ratings => _ratings.AsReadOnly();

        /// <summary>
        /// Calculated average rating (0 if no ratings).
        /// </summary>
        public double AverageRating
        {
            get
            {
                if (!_ratings.Any())
                    return 0;
                return Math.Round(_ratings.Average(r => r.Value), 2);
            }
        }

        /// <summary>
        /// Total number of ratings received.
        /// </summary>
        public int RatingCount => _ratings.Count;

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        public Idea() { }

        /// <summary>
        /// Creates a new idea with provided details.
        /// </summary>
        /// <param name="title">Idea title</param>
        /// <param name="description">Idea description</param>
        /// <param name="userId">Creator user ID</param>
        /// <param name="isAnonymous">Whether to post anonymously</param>
        /// <param name="teamId">Team ID if posting to a team</param>
        public Idea(string title, string description, string userId, bool isAnonymous = false, Guid? teamId = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            UserId = userId;
            IsAnonymous = isAnonymous;
            TeamId = teamId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a rating to this idea.
        /// Prevents duplicate ratings from the same user (domain invariant).
        /// </summary>
        /// <param name="rating">Rating to add</param>
        /// <returns>True if rating was added, false if duplicate user rating</returns>
        public bool AddRating(Rating rating)
        {
            if (rating == null)
                throw new ArgumentNullException(nameof(rating));

            // Prevent duplicate ratings from same user
            if (_ratings.Any(r => r.UserId == rating.UserId))
                return false;

            _ratings.Add(rating);
            return true;
        }

        /// <summary>
        /// Updates an existing rating from a user.
        /// </summary>
        /// <param name="userId">User ID providing the rating</param>
        /// <param name="newValue">New rating value (1-5)</param>
        /// <returns>True if updated, false if no existing rating found</returns>
        public bool UpdateRating(string userId, int newValue)
        {
            if (newValue < 1 || newValue > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(newValue));

            var existingRating = _ratings.FirstOrDefault(r => r.UserId == userId);
            if (existingRating == null)
                return false;

            existingRating.Value = newValue;
            existingRating.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Removes a rating from this idea.
        /// </summary>
        /// <param name="ratingId">Rating ID to remove</param>
        /// <returns>True if removed, false if not found</returns>
        public bool RemoveRating(Guid ratingId)
        {
            var rating = _ratings.FirstOrDefault(r => r.Id == ratingId);
            if (rating == null)
                return false;

            _ratings.Remove(rating);
            return true;
        }

        /// <summary>
        /// Gets the rating for a specific user.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>Rating value (1-5) or null if not rated by user</returns>
        public int? GetUserRating(string userId)
        {
            return _ratings.FirstOrDefault(r => r.UserId == userId)?.Value;
        }

        /// <summary>
        /// Checks if idea has been rated by a specific user.
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user has rated this idea</returns>
        public bool HasUserRated(string userId)
        {
            return _ratings.Any(r => r.UserId == userId);
        }

        /// <summary>
        /// Validates that the creator is not rating their own idea.
        /// </summary>
        /// <param name="userId">User attempting to rate</param>
        /// <returns>True if user can rate (is not the creator)</returns>
        public bool CanUserRate(string userId)
        {
            return UserId != userId;
        }

        /// <summary>
        /// Updates the basic idea information.
        /// </summary>
        /// <param name="title">New title</param>
        /// <param name="description">New description</param>
        public void Update(string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            Title = title;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}

