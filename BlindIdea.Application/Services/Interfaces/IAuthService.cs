using BlindIdea.Core.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using BlindIdea.Application.Dtos;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Service for handling user authentication including registration, login, token refresh, and email verification.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with email and password.
        /// Creates an EmailConfirmed=false user and sends email verification.
        /// </summary>
        /// <param name="dto">Registration details including name, email, password</param>
        /// <returns>AuthResponseDto with access and refresh tokens, or null if registration fails</returns>
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Authenticates user with email and password.
        /// Returns access and refresh tokens if credentials are valid.
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <param name="ipAddress">IP address of the login request (for audit trail)</param>
        /// <returns>AuthResponseDto with tokens, or null if authentication fails</returns>
        Task<AuthResponseDto?> LoginAsync(LoginDto dto, string ipAddress);

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Implements token rotation: revokes old refresh token and issues new one.
        /// </summary>
        /// <param name="refreshToken">The refresh token from client</param>
        /// <param name="ipAddress">IP address of the refresh request (for audit trail)</param>
        /// <returns>AuthResponseDto with new tokens, or null if refresh token is invalid/expired</returns>
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress);

        /// <summary>
        /// Logs out user by revoking their current refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to revoke</param>
        /// <param name="ipAddress">IP address of the logout request (for audit trail)</param>
        /// <returns>Success status</returns>
        Task<bool> LogoutAsync(string refreshToken, string ipAddress);

        /// <summary>
        /// Revokes ALL refresh tokens for a user (logout from all devices).
        /// Used for security breach recovery or account reset.
        /// </summary>
        /// <param name="userId">The user ID whose tokens should be revoked</param>
        /// <param name="ipAddress">IP address of the revocation request (for audit trail)</param>
        /// <returns>Success status</returns>
        Task<bool> RevokeAllTokensAsync(string userId, string ipAddress);

        /// <summary>
        /// Verifies email address using a verification token.
        /// Marks EmailConfirmed=true on successful verification.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="token">The verification token sent to email</param>
        /// <returns>Success status</returns>
        Task<bool> VerifyEmailAsync(string userId, string token);

        /// <summary>
        /// Resends email verification token to unverified users.
        /// Rate-limited: max once per 2 minutes per user.
        /// </summary>
        /// <param name="email">The user email</param>
        /// <returns>Success status</returns>
        Task<bool> ResendVerificationEmailAsync(string email);
    }
}

