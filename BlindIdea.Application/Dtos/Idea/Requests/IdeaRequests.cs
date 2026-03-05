using System;
using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Idea.Requests
{
    
    public class CreateIdeaRequest
    {
        
        [Required(ErrorMessage = "Idea title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Idea description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = null!;

        public bool IsAnonymous { get; set; } = false;

        public Guid? TeamId { get; set; }
    }

    public class UpdateIdeaRequest
    {
        
        [Required(ErrorMessage = "Idea title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Idea description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = null!;

        public bool IsAnonymous { get; set; }
    }

    public class SearchIdeasRequest
    {
        
        public string? SearchTerm { get; set; }

        public Guid? TeamId { get; set; }

        public bool CurrentUserOnly { get; set; } = false;

        public string SortBy { get; set; } = "newest";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}