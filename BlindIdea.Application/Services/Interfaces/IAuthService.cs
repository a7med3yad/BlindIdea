using BlindIdea.Application.Dtos.Auth.Requests;
using BlindIdea.Application.Dtos.Auth.Responses;
using BlindIdea.Application.Dtos.User;
using System;
using System.Threading.Tasks;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Authentication service for user registration, login, token management, and email verification.
    /// Implements production-grade security practices including token rotation and audit logging.
    /// </summary>
    public interface IAuthService
    {
        // ===== REGISTRATION & LOGIN =====

        /// <summary>
        /// Registers a new user with email and password.
        /// Creates user with EmailConfirmed=false and sends verification email.
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <param name="ipAddress">IP address of the registration request (for audit)</param>
        /// <returns>AuthResponse with tokens, or null if registration fails</returns>
        Task<AuthResponse?> RegisterAsync(RegisterRequest request, string? ipAddress = null);

        /// <summary>
        /// Authenticates user with email and password.
        /// Returns access and refresh tokens on success.
        /// User's email must be verified before login.
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <param name="ipAddress">IP address of the login request (for audit)</param>
        /// <returns>AuthResponse with tokens, or null if authentication fails</returns>
        Task<AuthResponse?> LoginAsync(LoginRequest request, string? ipAddress = null);

        // ===== TOKEN MANAGEMENT =====

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Implements automatic token rotation: revokes old token and issues new one.
        /// Detects token family for security breach detection.
        /// </summary>
        /// <param name="request">RefreshTokenRequest containing the refresh token</param>
        /// <param name="ipAddress">IP address of the refresh request (for audit)</param>
        /// <returns>AuthResponse with new tokens, or null if refresh token invalid/expired</returns>
        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null);

        /// <summary>
        /// Logs out user by revoking their current refresh token.
        /// Invalidates only the specified token, not all tokens.
        /// </summary>
        /// <param name="request">LogoutRequest with refresh token to revoke</param>
        /// <param name="ipAddress">IP address of the logout request (for audit)</param>
        /// <returns>True if logout successful, false if token not found</returns>
        Task<bool> LogoutAsync(LogoutRequest request, string? ipAddress = null);

        /// <summary>
        /// Revokes ALL refresh tokens for a user (logout from all devices).
        /// Used for password change, security breach recovery, or account lockdown.
        /// </summary>
        /// <param name="userId">The user ID whose tokens should be revoked</param>
        /// <param name="ipAddress">IP address of the revocation request (for audit)</param>
        /// <returns>True if successful, false if user not found</returns>
        Task<bool> RevokeAllTokensAsync(string userId, string? ipAddress = null);

        /// <summary>
        /// Revokes a specific refresh token by ID.
        /// Used for fine-grained token revocation.
        /// </summary>
        /// <param name="refreshTokenId">ID of refresh token to revoke</param>
        /// <param name="ipAddress">IP address of revocation request (for audit)</param>
        /// <returns>True if revoked, false if token not found</returns>
        Task<bool> RevokeTokenAsync(Guid refreshTokenId, string? ipAddress = null);

        // ===== EMAIL VERIFICATION =====

        /// <summary>
        /// Verifies user's email address using verification token.
        /// Marks EmailConfirmed=true on successful verification.
        /// Can only use unexpired, unused verification tokens.
        /// </summary>
        /// <param name="request">VerifyEmailRequest with user ID and token</param>
        /// <returns>VerifyEmailResponse with success status and message</returns>
        Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);

        /// <summary>
        /// Resends email verification token to unverified users.
        /// Rate-limited: allows resend max once per 2 minutes per user.
        /// </summary>
        /// <param name="request">ResendVerificationEmailRequest with email</param>
        /// <returns>True if email sent successfully</returns>
        Task<bool> ResendVerificationEmailAsync(ResendVerificationEmailRequest request);

        // ===== PASSWORD MANAGEMENT =====

        /// <summary>
        /// Changes user's password (requires current password verification).
        /// Invalidates all existing refresh tokens after password change.
        /// </summary>
        /// <param name="userId">User ID changing password</param>
        /// <param name="currentPassword">Current password for verification</param>
        /// <param name="newPassword">New password (must meet strength requirements)</param>
        /// <returns>True if password changed successfully</returns>
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Initiates password reset for user by email.
        /// Sends password reset email with secure token.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>True if reset email sent, false if user not found</returns>
        Task<bool> RequestPasswordResetAsync(string email);

        /// <summary>
        /// Resets user's password using reset token.
        /// Token must be valid and not expired.
        /// Invalidates all refresh tokens after reset.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="token">Password reset token</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password reset successful</returns>
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

        // ===== USER ACCOUNT =====

        /// <summary>
        /// Gets current authenticated user's profile.
        /// </summary>
        /// <param name="userId">User ID to retrieve</param>
        /// <returns>User data or null if not found</returns>
        Task<UserDetailResponse?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Updates user profile information.
        /// </summary>
        /// <param name="userId">User ID to update</param>
        /// <param name="updates">Field updates (only non-null fields)</param>
        /// <returns>Updated user data</returns>
        Task<UserDetailResponse?> UpdateProfileAsync(string userId, UpdateUserProfileRequest updates);

        /// <summary>
        /// Checks if an email address is already registered.
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if email exists, false otherwise</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Validates email format.
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if valid email format</returns>
        bool IsValidEmailFormat(string email);
    }
}

