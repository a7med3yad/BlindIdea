using System;

namespace BlindIdea.Application.Dtos.Auth.Responses
{
    /// <summary>
    /// Response payload for authentication endpoints (register, login, refresh).
    /// Contains tokens and user information.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Short-lived JWT access token (15 minutes by default).
        /// Include in Authorization header as: "Bearer {AccessToken}"
        /// Used for all subsequent API requests.
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Long-lived refresh token (7-30 days).
        /// Store securely (HttpOnly cookie or secure storage).
        /// Use only for refresh token endpoint, NOT for API requests.
        /// </summary>
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// Unix timestamp when access token expires.
        /// Client should request new access token before this time.
        /// </summary>
        public long AccessTokenExpiresIn { get; set; }

        /// <summary>
        /// Unix timestamp when refresh token expires.
        /// After this time, user must login again.
        /// </summary>
        public long RefreshTokenExpiresIn { get; set; }

        /// <summary>
        /// Token type - always "Bearer" for JWT tokens.
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Authenticated user information.
        /// </summary>
        public UserResponse User { get; set; } = null!;
    }

    /// <summary>
    /// User information returned in auth response.
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// User's full name as displayed in the application.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Indicates if user's email has been verified.
        /// Unverified users may have limited access.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Optional team ID if user belongs to a team.
        /// Null if user is not in any team.
        /// </summary>
        public Guid? TeamId { get; set; }

        /// <summary>
        /// List of roles assigned to the user.
        /// Common values: "User", "TeamAdmin", "Admin"
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Response for email verification confirmation.
    /// </summary>
    public class VerifyEmailResponse
    {
        /// <summary>
        /// Indicates if email verification was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the verification result.
        /// </summary>
        public string Message { get; set; } = null!;
    }

    /// <summary>
    /// Response for password validation feedback.
    /// Lists specific password requirements that were not met.
    /// </summary>
    public class PasswordValidationResponse
    {
        /// <summary>
        /// Indicates if password meets all requirements.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of failed password requirements.
        /// E.g., "Must contain at least one uppercase letter"
        /// Empty list if password is valid.
        /// </summary>
        public List<string> FailedRequirements { get; set; } = new();

        /// <summary>
        /// Descriptive message about password strength.
        /// </summary>
        public string? Message { get; set; }
    }
}
