using BlindIdea.Application.Dtos.Auth;

namespace BlindIdea.Application.Services.Abstraction.Auth
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyOtpDto dto);
        Task<AuthResponseDto> LoginAsync(RegisterDto dto);
        Task<string> ForgotPasswordAsync(string email);
        Task<AuthResponseDto> VerifyResetAsync(VerifyOtpDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task LogoutAsync(RefreshTokenDto dto);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<AuthResponseDto> HandleOAuthLoginAsync(string email);
        Task AssignRoleAsync(string email, string role);
    }
}