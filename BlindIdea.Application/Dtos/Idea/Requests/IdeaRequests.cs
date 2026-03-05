using System;
using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Idea.Requests
{
    /// <summary>
    /// Request to create a new idea.
    /// Ideas can be personal or posted to a team.
    /// </summary>
    public class CreateIdeaRequest
    {
        /// <summary>
        /// Idea title (5-200 characters).
        /// Brief summary of the idea.
        /// </summary>
        [Required(ErrorMessage = "Idea title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed description of the idea (10-2000 characters).
        /// Should explain benefits, implementation, etc.
        /// </summary>
        [Required(ErrorMessage = "Idea description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = null!;

        /// <summary>
        /// Whether to post this idea anonymously.
        /// If true, the creator's identity is hidden from viewers.
        /// </summary>
        public bool IsAnonymous { get; set; } = false;

        /// <summary>
        /// Optional team ID if posting idea to a specific team.
        /// If null, idea is posted personally.
        /// </summary>
        public Guid? TeamId { get; set; }
    }

    /// <summary>
    /// Request to update an existing idea.
    /// Only the idea creator can update (or admins).
    /// </summary>
    public class UpdateIdeaRequest
    {
        /// <summary>
        /// Updated idea title.
        /// </summary>
        [Required(ErrorMessage = "Idea title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Updated idea description.
        /// </summary>
        [Required(ErrorMessage = "Idea description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = null!;

        /// <summary>
        /// Updated anonymous flag.
        /// Note: Changing this may not update already submitted ratings.
        /// </summary>
        public bool IsAnonymous { get; set; }
    }

    /// <summary>
    /// Request to filter and search ideas.
    /// Used for listing ideas with various filters.
    /// </summary>
    public class SearchIdeasRequest
    {
        /// <summary>
        /// Optional keyword to search in title and description.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Optional team ID to filter ideas from specific team.
        /// If null, shows all ideas.
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// Whether to show only ideas from current user.
        /// </summary>
        public bool CurrentUserOnly { get; set; } = false;

        /// <summary>
        /// Sort order: "newest", "oldest", "mostrated", "highest"
        /// </summary>
        public string SortBy { get; set; } = "newest";

        /// <summary>
        /// Page number for pagination (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of ideas per page.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
