using System;

namespace BlindIdea.Application.Dtos.Auth.Responses
{
    /// <summary>
    /// Response after successful authentication
    /// </summary>
    public class AuthResponse
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime AccessTokenExpiration { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public bool IsEmailVerified { get; set; }
    }

    /// <summary>
    /// Response for email verification
    /// </summary>
    public class VerifyEmailResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = null!;

        public DateTime? VerifiedAt { get; set; }
    }

    /// <summary>
    /// Response for resending verification email
    /// </summary>
    public class ResendVerificationEmailResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = null!;

        public string? Email { get; set; }
    }

    /// <summary>
    /// Response with user details
    /// </summary>
    public class UserDetailResponse
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool IsEmailVerified { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Guid? TeamId { get; set; }

        public int IdeaCount { get; set; }

        public int RatingCount { get; set; }
    }
}
