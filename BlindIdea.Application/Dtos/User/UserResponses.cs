using System;

namespace BlindIdea.Application.Dtos.User
{
    
    public class UserDetailResponse
    {
        
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool EmailConfirmed { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? TeamId { get; set; }

        public TeamBasicResponse? Team { get; set; }

        public List<string> Roles { get; set; } = new();

        public int IdeasCount { get; set; }

        public int RatingsCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool CanEdit { get; set; }
    }

    public class UserBasicResponse
    {
        
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;
    }

    public class UpdateUserProfileRequest
    {
        
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordRequest
    {
        
        public string CurrentPassword { get; set; } = null!;

        public string NewPassword { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;
    }

    public class TeamBasicResponse
    {
        
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
    }
}