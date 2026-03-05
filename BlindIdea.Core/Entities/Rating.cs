using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Entities
{
    /// <summary>
    /// Rating entity representing a user's evaluation of an idea.
    /// Ratings are numeric (1-5) and can be updated or removed.
    /// Prevents self-ratings and duplicate ratings from same user.
    /// </summary>
    public class Rating : BaseEntity
    {
        /// <summary>
        /// The numeric rating value.
        /// Must be between 1 (lowest) and 5 (highest).
        /// Enforced by database check constraint.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Optional comment justifying or explaining the rating.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Foreign key to the Idea being rated.
        /// </summary>
        public Guid IdeaId { get; set; }

        /// <summary>
        /// Navigation property to the Idea.
        /// </summary>
        public virtual Idea Idea { get; set; } = null!;

        /// <summary>
        /// Foreign key to the User providing the rating.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to the User.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        public Rating() { }

        /// <summary>
        /// Creates a new rating.
        /// </summary>
        /// <param name="ideaId">ID of the idea being rated</param>
        /// <param name="userId">ID of the user providing the rating</param>
        /// <param name="value">Rating value (1-5)</param>
        /// <param name="comment">Optional comment</param>
        public Rating(Guid ideaId, string userId, int value, string? comment = null)
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Rating value must be between 1 and 5", nameof(value));

            Id = Guid.NewGuid();
            IdeaId = ideaId;
            UserId = userId;
            Value = value;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the rating value is within acceptable range.
        /// </summary>
        /// <returns>True if valid, throws exception if invalid</returns>
        public bool Validate()
        {
            if (Value < 1 || Value > 5)
                throw new InvalidOperationException("Rating value must be between 1 and 5");

            return true;
        }

        /// <summary>
        /// Updates the rating value and optional comment.
        /// </summary>
        /// <param name="newValue">New rating value (1-5)</param>
        /// <param name="newComment">New comment (optional)</param>
        public void Update(int newValue, string? newComment = null)
        {
            if (newValue < 1 || newValue > 5)
                throw new ArgumentException("Rating value must be between 1 and 5", nameof(newValue));

            Value = newValue;
            Comment = newComment;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
