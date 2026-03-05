using BlindIdea.Application.Dtos.Auth.Requests;
using BlindIdea.Application.Dtos.Auth.Responses;
using BlindIdea.Application.Dtos.User;
using System;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request, string? ipAddress = null);

        Task<AuthResponse?> LoginAsync(LoginRequest request, string? ipAddress = null);

        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null);

        Task<bool> LogoutAsync(LogoutRequest request, string? ipAddress = null);

        Task<bool> RevokeAllTokensAsync(string userId, string? ipAddress = null);

        Task<bool> RevokeTokenAsync(Guid refreshTokenId, string? ipAddress = null);

        Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);

        Task<bool> ResendVerificationEmailAsync(ResendVerificationEmailRequest request);

        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        Task<bool> RequestPasswordResetAsync(string email);

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

        Task<UserDetailResponse?> GetUserProfileAsync(string userId);

        Task<UserDetailResponse?> UpdateProfileAsync(string userId, UpdateUserProfileRequest updates);

        Task<bool> EmailExistsAsync(string email);

        bool IsValidEmailFormat(string email);
    }
}