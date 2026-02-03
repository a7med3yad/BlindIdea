using BlindIdea.Infrastructure.Common.Options;
using BlindIdea.Application.Dtos;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Interfaces;

namespace BlindIdea.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly JwtOptions _jwtOptions;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
        }

        // ================= REGISTER =================
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return null;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.Email,
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return null;

            var token = _jwtService.CreateAccessToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(_jwtOptions.ExpireDays),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email!,
                }
            };
        }

        // ================= LOGIN =================
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, false);

            if (!result.Succeeded)
                return null;

            var token = _jwtService.CreateAccessToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(_jwtOptions.ExpireDays),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email!,
                }
            };
        }

        
    }
}
