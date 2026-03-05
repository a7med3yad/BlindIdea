using System;

namespace BlindIdea.Application.Dtos.Auth.Responses
{
    
    public class AuthResponse
    {
        
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public long AccessTokenExpiresIn { get; set; }

        public long RefreshTokenExpiresIn { get; set; }

        public string TokenType { get; set; } = "Bearer";

        public UserResponse User { get; set; } = null!;
    }

    public class UserResponse
    {
        
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool EmailConfirmed { get; set; }

        public Guid? TeamId { get; set; }

        public List<string> Roles { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }

    public class VerifyEmailResponse
    {
        
        public bool Success { get; set; }

        public string Message { get; set; } = null!;
    }

    public class PasswordValidationResponse
    {
        
        public bool IsValid { get; set; }

        public List<string> FailedRequirements { get; set; } = new();

        public string? Message { get; set; }
    }
}