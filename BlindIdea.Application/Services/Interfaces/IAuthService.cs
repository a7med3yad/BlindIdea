using BlindIdea.Core.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using BlindIdea.Application.Dtos;

namespace BlindIdea.Application.Services.Interfaces
{
    public interface IAuthService
    {

        Task RegisterAsync(RegisterDto dto);
        Task<(string accessToken, string refreshToken)> LoginAsync(LoginDto dto);
        Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task LogoutAllAsync(string userId);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task ConfirmEmailAsync(string userId, string token);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task ResetPasswordAsync(string email, string token, string newPassword);
    }
}
