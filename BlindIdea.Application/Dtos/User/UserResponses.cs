using System;

namespace BlindIdea.Application.Dtos.User
{
    /// <summary>
    /// Complete user information response.
    /// </summary>
    public class UserDetailResponse
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User's full name/display name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Whether user's email is verified.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// User's phone number if provided.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Team ID if user is in a team.
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// Team information if user is in a team.
        /// </summary>
        public TeamBasicResponse? Team { get; set; }

        /// <summary>
        /// List of roles assigned to the user.
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Number of ideas created by user.
        /// </summary>
        public int IdeasCount { get; set; }

        /// <summary>
        /// Number of ratings given by user.
        /// </summary>
        public int RatingsCount { get; set; }

        /// <summary>
        /// When the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the user account was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Whether the current user can edit this profile.
        /// </summary>
        public bool CanEdit { get; set; }
    }

    /// <summary>
    /// Basic user information for inclusion in other responses.
    /// </summary>
    public class UserBasicResponse
    {
        /// <summary>
        /// User ID.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User's display name.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// User profile update request.
    /// </summary>
    public class UpdateUserProfileRequest
    {
        /// <summary>
        /// Updated name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Updated phone number.
        /// </summary>
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Change password request.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Current password for verification.
        /// </summary>
        public string CurrentPassword { get; set; } = null!;

        /// <summary>
        /// New password (must meet strength requirements).
        /// </summary>
        public string NewPassword { get; set; } = null!;

        /// <summary>
        /// Confirmation of new password.
        /// </summary>
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// Basic team information.
    /// </summary>
    public class TeamBasicResponse
    {
        /// <summary>
        /// Team ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Team name.
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
