using BlindIdea.Application.Dtos;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers
{
    /// <summary>
    /// Authentication endpoints for user registration, login, token refresh, and email verification.
    /// Implements production-grade security practices with token rotation and email verification.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with email and password.
        /// Sends email verification link that user must click before login.
        /// </summary>
        /// <param name="dto">Registration details (name, email, password, confirmPassword)</param>
        /// <returns>AuthResponseDto with access and refresh tokens</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Registration failed (user exists, validation error)</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                // Validate DTOs
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate password match
                if (dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest("Passwords do not match.");
                }

                var authResponse = await _authService.RegisterAsync(dto);
                if (authResponse == null)
                {
                    _logger.LogWarning($"Registration failed for email: {dto.Email}");
                    return BadRequest("Registration failed. Email may already exist or password does not meet requirements.");
                }

                _logger.LogInformation($"User registered: {authResponse.User.Id}");
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in Register: {ex.Message}");
                return StatusCode(500, "An error occurred during registration.");
            }
        }

        /// <summary>
        /// Authenticates user with email and password.
        /// Returns access token (15 min) and refresh token (7 days).
        /// Email must be verified before login (see POST /auth/verify-email).
        /// </summary>
        /// <param name="dto">Login credentials (email, password, rememberMe)</param>
        /// <returns>AuthResponseDto with tokens</returns>
        /// <response code="200">Login successful</response>
        /// <response code="401">Authentication failed (invalid credentials, unverified email)</response>
        /// <response code="400">Bad request</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ipAddress = GetClientIpAddress();
                var authResponse = await _authService.LoginAsync(dto, ipAddress);

                if (authResponse == null)
                {
                    _logger.LogWarning($"Login failed for email: {dto.Email} from IP: {ipAddress}");
                    return Unauthorized(
                        new ProblemDetails
                        {
                            Status = 401,
                            Title = "Authentication Failed",
                            Detail = "Invalid credentials or email not verified. Please verify your email address before logging in."
                        });
                }

                _logger.LogInformation($"User logged in: {authResponse.User.Id} from IP: {ipAddress}");
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in Login: {ex.Message}");
                return StatusCode(500, "An error occurred during login.");
            }
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Implements token rotation: old refresh token is revoked, new one issued.
        /// </summary>
        /// <remarks>
        /// Token rotation provides enhanced security:
        /// - Old refresh token is marked as used
        /// - New refresh token is issued alongside new access token
        /// - If revoked token is reused (security breach), all tokens are revoked
        /// </remarks>
        /// <param name="dto">Refresh token request</param>
        /// <returns>AuthResponseDto with new tokens</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="401">Refresh token invalid or expired</response>
        /// <response code="400">Bad request</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                {
                    return BadRequest("Refresh token is required.");
                }

                var ipAddress = GetClientIpAddress();
                var authResponse = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress);

                if (authResponse == null)
                {
                    _logger.LogWarning($"Token refresh failed from IP: {ipAddress}");
                    return Unauthorized(
                        new ProblemDetails
                        {
                            Status = 401,
                            Title = "Invalid Refresh Token",
                            Detail = "The refresh token is invalid, expired, or has been revoked."
                        });
                }

                _logger.LogInformation($"Token refreshed for user: {authResponse.User.Id} from IP: {ipAddress}");
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in RefreshToken: {ex.Message}");
                return StatusCode(500, "An error occurred during token refresh.");
            }
        }

        /// <summary>
        /// Logs out user by revoking their current refresh token.
        /// This logs out the user on THIS device only.
        /// Use /auth/revoke-all to logout from all devices.
        /// </summary>
        /// <param name="dto">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        /// <response code="204">Logout successful</response>
        /// <response code="400">Bad request</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                {
                    return BadRequest("Refresh token is required.");
                }

                var ipAddress = GetClientIpAddress();
                var success = await _authService.LogoutAsync(dto.RefreshToken, ipAddress);

                if (!success)
                {
                    return BadRequest("Logout failed.");
                }

                _logger.LogInformation($"User logged out from IP: {ipAddress}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in Logout: {ex.Message}");
                return StatusCode(500, "An error occurred during logout.");
            }
        }

        /// <summary>
        /// Revokes ALL refresh tokens for the authenticated user.
        /// Logs user out from all devices/sessions.
        /// Requires valid JWT access token.
        /// </summary>
        /// <returns>Success status</returns>
        /// <response code="204">All tokens revoked successfully</response>
        /// <response code="401">Unauthorized (missing or invalid token)</response>
        [Authorize]
        [HttpPost("revoke-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeAllTokens()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                var ipAddress = GetClientIpAddress();
                var success = await _authService.RevokeAllTokensAsync(userId, ipAddress);

                if (!success)
                {
                    return StatusCode(500, "Failed to revoke tokens.");
                }

                _logger.LogWarning($"All tokens revoked for user: {userId} from IP: {ipAddress}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in RevokeAllTokens: {ex.Message}");
                return StatusCode(500, "An error occurred while revoking tokens.");
            }
        }

        /// <summary>
        /// Verifies user email address using verification token.
        /// Token is sent via email during registration.
        /// Once verified, user can login to their account.
        /// </summary>
        /// <remarks>
        /// The verification URL is sent to the user's email:
        /// GET /auth/verify-email?userId={userId}&token={token}
        ///
        /// Query Parameters:
        /// - userId: The user ID (required)
        /// - token: The verification token from email (required, must be URL-decoded)
        /// </remarks>
        /// <param name="userId">User ID</param>
        /// <param name="token">Verification token from email</param>
        /// <returns>Redirect to success/error page (client should handle)</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Verification failed (invalid/expired token)</response>
        [HttpGet("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest("UserId and verification token are required.");
                }

                // URL decode the token (browser sends it encoded)
                var decodedToken = Uri.UnescapeDataString(token);

                var success = await _authService.VerifyEmailAsync(userId, decodedToken);

                if (!success)
                {
                    _logger.LogWarning($"Email verification failed for user: {userId}");
                    return BadRequest(
                        new ProblemDetails
                        {
                            Status = 400,
                            Title = "Email Verification Failed",
                            Detail = "The verification token is invalid, expired, or has already been used."
                        });
                }

                _logger.LogInformation($"Email verified successfully for user: {userId}");
                return Ok(new { message = "Email verified successfully. You can now login." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in VerifyEmail: {ex.Message}");
                return StatusCode(500, "An error occurred during email verification.");
            }
        }

        /// <summary>
        /// Resends email verification token to unverified users.
        /// Rate-limited: maximum once per 2 minutes per user.
        /// </summary>
        /// <remarks>
        /// Use this if the user did not receive the original verification email
        /// or if the token expired (24 hour expiry).
        /// </remarks>
        /// <param name="dto">User email</param>
        /// <returns>Success message</returns>
        /// <response code="200">Verification email sent</response>
        /// <response code="400">Bad request or rate limited</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return BadRequest("Email is required.");
                }

                var success = await _authService.ResendVerificationEmailAsync(dto.Email);

                if (!success)
                {
                    _logger.LogWarning($"Resend verification failed for email: {dto.Email}");
                    return BadRequest(
                        new ProblemDetails
                        {
                            Status = 400,
                            Title = "Resend Verification Failed",
                            Detail = "Email not found, already verified, or too many resend requests (max once per 2 minutes)."
                        });
                }

                _logger.LogInformation($"Verification email resent to: {dto.Email}");
                return Ok(new { message = "Verification email sent. Please check your inbox." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in ResendVerificationEmail: {ex.Message}");
                return StatusCode(500, "An error occurred while sending verification email.");
            }
        }

        // ============= HELPER METHODS =============

        /// <summary>
        /// Extracts client IP address from request headers.
        /// Checks X-Forwarded-For header first (for proxies), then RemoteIpAddress.
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedForHeader))
                {
                    var ipAddress = forwardedForHeader.ToString().Split(',').First().Trim();
                    return ipAddress;
                }

                return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }
    }
}
