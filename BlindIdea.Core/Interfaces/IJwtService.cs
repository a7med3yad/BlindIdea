using BlindIdea.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Core.Interfaces
{
    /// <summary>
    /// Service for JWT token generation, validation, and management.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Creates a short-lived access token for API authentication.
        /// </summary>
        /// <param name="user">The user to create token for</param>
        /// <returns>Encoded JWT access token</returns>
        string CreateAccessToken(User user);

        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// Must be hashed before storage.
        /// </summary>
        /// <returns>Base64-encoded refresh token (plain text)</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Hashes a token using SHA-256 for secure storage.
        /// </summary>
        /// <param name="token">Plain text token to hash</param>
        /// <returns>SHA-256 hex string hash</returns>
        string HashToken(string token);

        /// <summary>
        /// Extracts the JWT ID (jti) claim from a token.
        /// </summary>
        /// <param name="token">Encoded JWT token</param>
        /// <returns>The jti claim value, or null if not found</returns>
        string? ExtractJwtId(string token);
    }
}
