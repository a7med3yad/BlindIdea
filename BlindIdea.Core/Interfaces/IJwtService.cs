using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlindIdea.Core.Interfaces
{
    /// <summary>
    /// Interface for JWT token generation and validation services
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate an JWT access token
        /// </summary>
        string GenerateAccessToken(string userId, string email, string name, Dictionary<string, string>? claims = null);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate an JWT token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Get claims from a token
        /// </summary>
        List<Claim>? GetTokenClaims(string token);

        /// <summary>
        /// Generate an email verification token
        /// </summary>
        string GenerateEmailVerificationToken(string userId);

        /// <summary>
        /// Verify an email verification token
        /// </summary>
        Task<bool> VerifyEmailTokenAsync(string userId, string token);

        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// Check if a token is revoked
        /// </summary>
        Task<bool> IsTokenRevokedAsync(string token);
    }
}
