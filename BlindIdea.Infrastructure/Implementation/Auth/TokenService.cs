using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BlindIdea.Infrastructure.Persistence;
using BlindIdea.Domain.Entities;
using BlindIdea.Domain.Abstraction.Services;

namespace BlindIdea.Infrastructure.Implementation.Auth
{
    public class TokenService:ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public TokenService(IConfiguration configuration, AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }
        public async Task<string> CreateAccessToken(ApplicationUser user)
        {
            if (user == null)
                throw new Exception("User is null");

            var freshUser = await _userManager.FindByIdAsync(user.Id)
                ?? throw new Exception($"User not found by Id: {user.Id}");

            if (string.IsNullOrEmpty(freshUser.Email))
                throw new Exception($"User email is null. UserId: {freshUser.Id}");

            var roles = await _userManager.GetRolesAsync(freshUser);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, freshUser.Id),
        new Claim(ClaimTypes.Email, freshUser.Email)
    };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> CreateRefreshToken(ApplicationUser user)
        {
            if (user == null)
                throw new Exception("User is null in CreateRefreshToken");

            try
            {
                var trackedUser = _context.ChangeTracker.Entries<ApplicationUser>()
                    .FirstOrDefault(e => e.Entity.Id == user.Id);

                if (trackedUser != null)
                    trackedUser.State = EntityState.Detached;

                var refreshToken = new RefreshToken
                {
                    Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false,
                    UserId = user.Id
                };

                // ✅ Revoke old tokens
                var oldTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == user.Id && !t.IsRevoked)
                    .ToListAsync();

                foreach (var old in oldTokens)
                    old.IsRevoked = true;

                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                return refreshToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"CreateRefreshToken failed: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

    }
}
