using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Auth.Requests
{
    /// <summary>
    /// Request for user registration
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// Request for user login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Request for logout
    /// </summary>
    public class LogoutRequest
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// Request for email verification
    /// </summary>
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Verification token is required")]
        public string Token { get; set; } = null!;
    }

    /// <summary>
    /// Request to resend verification email
    /// </summary>
    public class ResendVerificationEmailRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// Request to update user profile
    /// </summary>
    public class UpdateUserProfileRequest
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
    }
}
