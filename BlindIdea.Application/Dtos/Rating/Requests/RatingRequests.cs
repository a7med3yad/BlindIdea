using System;
using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Rating.Requests
{
    /// <summary>
    /// Request to rate an idea.
    /// Users can rate ideas 1-5 stars with optional comments.
    /// Users cannot rate their own ideas.
    /// </summary>
    public class CreateRatingRequest
    {
        /// <summary>
        /// The idea ID being rated.
        /// </summary>
        [Required(ErrorMessage = "Idea ID is required")]
        public Guid IdeaId { get; set; }

        /// <summary>
        /// The rating value (1-5 stars).
        /// 1 = Poor, 5 = Excellent.
        /// </summary>
        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Value { get; set; }

        /// <summary>
        /// Optional comment explaining the rating.
        /// </summary>
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Request to update a rating.
    /// Only the user who gave the rating can update it.
    /// </summary>
    public class UpdateRatingRequest
    {
        /// <summary>
        /// The new rating value (1-5).
        /// </summary>
        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Value { get; set; }

        /// <summary>
        /// Updated comment.
        /// </summary>
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Request to delete a rating.
    /// Only the user who gave the rating can delete it.
    /// </summary>
    public class DeleteRatingRequest
    {
        /// <summary>
        /// The rating ID to delete.
        /// </summary>
        [Required(ErrorMessage = "Rating ID is required")]
        public Guid RatingId { get; set; }
    }

    /// <summary>
    /// Request to filter and list ratings for an idea.
    /// </summary>
    public class ListRatingsRequest
    {
        /// <summary>
        /// Idea ID to get ratings for.
        /// </summary>
        [Required(ErrorMessage = "Idea ID is required")]
        public Guid IdeaId { get; set; }

        /// <summary>
        /// Optional filter by rating value.
        /// Null to show all ratings.
        /// </summary>
        public int? RatingValue { get; set; }

        /// <summary>
        /// Sort order: "newest", "oldest", "highest", "lowest"
        /// </summary>
        public string SortBy { get; set; } = "newest";

        /// <summary>
        /// Page number for pagination.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of ratings per page.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
