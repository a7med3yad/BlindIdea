using System;
using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Rating.Requests
{
    
    public class CreateRatingRequest
    {
        
        [Required(ErrorMessage = "Idea ID is required")]
        public Guid IdeaId { get; set; }

        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Value { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

    public class UpdateRatingRequest
    {
        
        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Value { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

    public class DeleteRatingRequest
    {
        
        [Required(ErrorMessage = "Rating ID is required")]
        public Guid RatingId { get; set; }
    }

    public class ListRatingsRequest
    {
        
        [Required(ErrorMessage = "Idea ID is required")]
        public Guid IdeaId { get; set; }

        public int? RatingValue { get; set; }

        public string SortBy { get; set; } = "newest";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}