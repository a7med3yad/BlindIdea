using System;
using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Team.Requests
{
    /// <summary>
    /// Request to create a new team.
    /// Only authenticated users can create teams.
    /// </summary>
    public class CreateTeamRequest
    {
        /// <summary>
        /// Team name (3-100 characters, must be unique).
        /// </summary>
        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Team name must be between 3 and 100 characters")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Optional team description explaining the team's purpose.
        /// </summary>
        [StringLength(500, ErrorMessage = "Team description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request to update an existing team.
    /// Only team admin can update team details.
    /// </summary>
    public class UpdateTeamRequest
    {
        /// <summary>
        /// New team name.
        /// </summary>
        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Team name must be between 3 and 100 characters")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// New team description.
        /// </summary>
        [StringLength(500, ErrorMessage = "Team description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request to add a user to a team.
    /// Only team admin can add members.
    /// </summary>
    public class AddTeamMemberRequest
    {
        /// <summary>
        /// User ID to add to the team.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;
    }

    /// <summary>
    /// Request to remove a user from a team.
    /// Only team admin can remove members (except removing themselves).
    /// </summary>
    public class RemoveTeamMemberRequest
    {
        /// <summary>
        /// User ID to remove from the team.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;
    }

    /// <summary>
    /// Request to transfer team admin to another user.
    /// Only current admin can transfer admin role.
    /// </summary>
    public class TransferAdminRequest
    {
        /// <summary>
        /// User ID of the new admin.
        /// Must be an existing member of the team.
        /// </summary>
        [Required(ErrorMessage = "New admin user ID is required")]
        public string NewAdminId { get; set; } = null!;
    }
}
