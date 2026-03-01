using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlindIdea.Infrastructure.Services
{
    /// <summary>
    /// Service for generating and validating JWT tokens and refresh tokens.
    /// Implements production-grade security practices for token management.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        /// <summary>
        /// Creates a short-lived access token (15 minutes by default) for API authentication.
        /// </summary>
        /// <remarks>
        /// Token includes claims:
        /// - NameIdentifier: User ID
        /// - Email: User email
        /// - Name: User name
        /// - JwtId (jti): Unique token ID for audit and token rotation
        /// - Role: User roles (if applicable)
        /// </remarks>
        /// <param name="user">The user entity to create token for</param>
        /// <returns>Encoded JWT access token</returns>
        public string CreateAccessToken(User user)
        {
            var jwtId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("jti", jwtId), // JWT ID for token rotation tracking
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha512);

            var expiryMinutes = _jwtOptions.AccessTokenExpiryMinutes > 0
                ? _jwtOptions.AccessTokenExpiryMinutes
                : 15; // Default to 15 minutes

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// Token is base64-encoded and should be hashed before storage.
        /// </summary>
        /// <remarks>
        /// Token is 32 bytes of random data (256 bits), providing excellent entropy.
        /// Must be hashed with HashToken() before storing in the database.
        /// </remarks>
        /// <returns>Base64-encoded refresh token (plain text, not hashed)</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Hashes a token using SHA-256 for secure storage in the database.
        /// Plain tokens should never be stored; always hash before persistence.
        /// </summary>
        /// <remarks>
        /// Uses SHA-256 for consistency with industry standards.
        /// Called before storing refresh tokens or verification tokens in DB.
        /// </remarks>
        /// <param name="token">Plain text token to hash</param>
        /// <returns>SHA-256 hex string hash of the token</returns>
        public string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToHexString(hashedBytes);
            }
        }

        /// <summary>
        /// Extracts the JWT ID (jti) claim from a JWT token string.
        /// Used to link refresh tokens with their corresponding access tokens.
        /// </summary>
        /// <param name="token">Encoded JWT token</param>
        /// <returns>The jti claim value, or null if not found</returns>
        public string? ExtractJwtId(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                return jwtToken?.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
