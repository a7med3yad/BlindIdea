using System.ComponentModel.DataAnnotations;

namespace BlindIdea.Application.Dtos.Auth.Requests
{
    /// <summary>
    /// Request payload for user registration.
    /// Validates input and password strength requirements.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// User's full name (2-100 characters).
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address (must be unique).
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// User's password (must meet strength requirements).
        /// Minimum 8 characters, uppercase, lowercase, digit, special character.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 256 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Password confirmation (must match Password).
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// Request payload for user login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// User's password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Flag to remember this user for extended session (30 days).
        /// If false, standard 7-day refresh token duration applies.
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Request payload for refreshing access token.
    /// Requires a valid refresh token.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The refresh token from previous authentication.
        /// Must be valid and not expired/revoked.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// The client's IP address (auto-set from request).
        /// Used for audit trail and security purposes.
        /// </summary>
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// Request payload for verifying email address.
    /// </summary>
    public class VerifyEmailRequest
    {
        /// <summary>
        /// The user ID to verify email for.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// The email verification token sent to user's email.
        /// </summary>
        [Required(ErrorMessage = "Verification token is required")]
        public string Token { get; set; } = null!;
    }

    /// <summary>
    /// Request payload for resending email verification.
    /// </summary>
    public class ResendVerificationEmailRequest
    {
        /// <summary>
        /// The user's email address to resend verification to.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// Request payload for user logout.
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// The refresh token to revoke.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = null!;
    }

    /// <summary>
    /// Request payload for changing user password.
    /// Requires verification of current password.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// User's current password (must be correct).
        /// </summary>
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = null!;

        /// <summary>
        /// User's new password (must meet strength requirements).
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 256 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        /// <summary>
        /// Confirmation of new password (must match NewPassword).
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    /// <summary>
    /// Request payload for initiating password reset.
    /// Sends reset token via email.
    /// </summary>
    public class RequestPasswordResetRequest
    {
        /// <summary>
        /// Email address to send password reset link to.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// Request payload for completing password reset.
    /// Uses token sent via email.
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// The user ID to reset password for.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// The password reset token from email.
        /// </summary>
        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; } = null!;

        /// <summary>
        /// The new password (must meet strength requirements).
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 256 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        /// <summary>
        /// Confirmation of new password (must match NewPassword).
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
