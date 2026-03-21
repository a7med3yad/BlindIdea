using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Dtos.Auth;
using BlindIdea.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlindIdea.Infrastructure.Implementation.Auth
{
    public class OAuthService:IOAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public OAuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> HandleOAuthLogin(AuthenticateResult result)
        {
            if (result?.Principal == null)
                throw new Exception("Authentication result or principal is null");

            var email =
                result.Principal.FindFirst(ClaimTypes.Email)?.Value ??
                result.Principal.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
                throw new Exception("Email not found from OAuth provider");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    IsVerified = true
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                    throw new Exception($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                await _userManager.AddToRoleAsync(user, "User");
            }

            user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("User not found after creation");

            string accessToken;
            string refreshToken;

            try
            {
                accessToken = await _tokenService.CreateAccessToken(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"AccessToken failed: {ex.Message}");
            }

            try
            {
                refreshToken = (await _tokenService.CreateRefreshToken(user)).Token;
            }
            catch (Exception ex)
            {
                throw new Exception($"RefreshToken failed: {ex.Message}");
            }

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}