using BlindIdea.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using BlindIdea.API.Dtos;

namespace BlindIdea.API.Services
{
    public class OAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        public OAuthService(UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> HandleOAuthLogin(AuthenticateResult result)
        {
            var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;

            if(string.IsNullOrEmpty(email))
            {
                throw new Exception("Email not provided by OAuth provider");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if(user== null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    IsVerified = true
                };
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, "User");
            }
            return new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            };

        }
    }
}
