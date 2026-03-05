using BlindIdea.Application.Dtos.Auth.Requests;
using BlindIdea.Application.Dtos.Auth.Responses;
using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Dtos.User;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlindIdea.API.Controllers
{
    /// <summary>
    /// Authentication endpoints for user registration, login, token management, and email verification.
    /// Implements production-grade security practices with token rotation, refresh token revocation, and email verification.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IPasswordValidator _passwordValidator;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Constructor injecting required services.
        /// </summary>
        public AuthController(
            IAuthService authService,
            IPasswordValidator passwordValidator,
            ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ===== REGISTRATION =====

        /// <summary>
        /// Registers a new user with email and password.
        /// Sends email verification link that must be clicked before login.
        /// </summary>
        /// <remarks>
        /// **Password Requirements:**
        /// - Minimum 8 characters
        /// - At least one uppercase letter (A-Z)
        /// - At least one lowercase letter (a-z)
        /// - At least one number (0-9)
        /// - At least one special character (!@#$%^&*_+=-[]{}';:\"\\|,.<>/?")
        /// </remarks>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "Validation failed",
                        statusCode: 400
                    ));
                }

                // Validate password strength
                var passwordValidation = _passwordValidator.Validate(request.Password);
                if (!passwordValidation.IsValid)
                {
                    _logger.LogWarning($"Registration rejected: weak password for email {request.Email}");
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        $"Password does not meet requirements: {passwordValidation.ErrorMessage}",
                        statusCode: 400
                    ));
                }

                // Register user
                var response = await _authService.RegisterAsync(request);
                
                _logger.LogInformation($"User registered successfully: {response.User.Id}");
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "User registered successfully", 200));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Registration validation error: {ex.Message}");
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                throw;
            }
        }

        // ===== LOGIN =====

        /// <summary>
        /// Authenticates user with email and password.
        /// Returns access token (15 min expiry) and refresh token (7 days expiry).
        /// Email must be verified before successful login.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "Validation failed",
                        statusCode: 400
                    ));
                }

                var response = await _authService.LoginAsync(request);
                
                _logger.LogInformation($"User logged in successfully: {response.User.Id}");
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                return Unauthorized(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 401));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Login validation error: {ex.Message}");
                return Unauthorized(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 401));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                throw;
            }
        }

        // ===== TOKEN MANAGEMENT =====

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Implements token rotation: old token is revoked, new tokens are issued.
        /// Detects and prevents replay attacks by revoking all tokens if revoked token is reused.
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "Refresh token is required",
                        statusCode: 400
                    ));
                }

                request.IpAddress = GetClientIpAddress();
                var response = await _authService.RefreshTokenAsync(request);
                
                _logger.LogInformation($"Token refreshed successfully");
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Token refresh failed: {ex.Message}");
                return Unauthorized(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 401));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh error");
                throw;
            }
        }

        /// <summary>
        /// Logs out the user by revoking their current refresh token.
        /// Prevents the token from being used again.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                await _authService.LogoutAsync(request, ipAddress);
                
                _logger.LogInformation("User logged out successfully");
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { message = "Logout successful" },
                    "Logout successful",
                    200
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                throw;
            }
        }

        /// <summary>
        /// Revokes all refresh tokens for the current user.
        /// Effectively logs out user from all devices/sessions.
        /// </summary>
        [HttpPost("revoke-all")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeAll()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.FailureResponse(
                        "User not found in token",
                        statusCode: 401
                    ));
                }

                await _authService.RevokeAllTokensAsync(userId);
                
                _logger.LogInformation($"All tokens revoked for user: {userId}");
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { message = "All tokens revoked successfully" },
                    "All tokens revoked",
                    200
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Revoke all error");
                throw;
            }
        }

        // ===== EMAIL VERIFICATION =====

        /// <summary>
        /// Verifies user's email address using the verification token.
        /// Email must be verified before user can login.
        /// </summary>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "User ID and token are required",
                        statusCode: 400
                    ));
                }

                var response = await _authService.VerifyEmailAsync(request);
                
                _logger.LogInformation($"Email verified for user: {request.UserId}");
                return Ok(ApiResponse<VerifyEmailResponse>.SuccessResponse(response, "Email verified successfully", 200));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Email verification failed: {ex.Message}");
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification error");
                throw;
            }
        }

        /// <summary>
        /// Resends email verification token to unverified users.
        /// Rate-limited: maximum once per 2 minutes per user.
        /// </summary>
        [HttpPost("resend-verification")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "Email is required",
                        statusCode: 400
                    ));
                }

                await _authService.ResendVerificationEmailAsync(request);
                
                _logger.LogInformation($"Verification email resent to: {request.Email}");
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { message = "Verification email sent if email exists" },
                    "Verification email sent",
                    200
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Resend verification failed: {ex.Message}");
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend verification error");
                throw;
            }
        }

        // ===== PASSWORD MANAGEMENT =====

        /// <summary>
        /// Changes password for the authenticated user.
        /// Requires verification of current password before accepting new password.
        /// All refresh tokens are revoked after successful password change.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] BlindIdea.Application.Dtos.Auth.Requests.ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "Validation failed",
                        statusCode: 400
                    ));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.FailureResponse(
                        "User not found in token",
                        statusCode: 401
                    ));
                }

                // Validate new password strength
                var passwordValidation = _passwordValidator.Validate(request.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        $"New password does not meet requirements: {passwordValidation.ErrorMessage}",
                        statusCode: 400
                    ));
                }

                await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                
                _logger.LogInformation($"Password changed for user: {userId}");
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { message = "Password changed successfully" },
                    "Password changed successfully",
                    200
                ));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Password change unauthorized: {ex.Message}");
                return Unauthorized(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 401));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Password change failed: {ex.Message}");
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password change error");
                throw;
            }
        }

        /// <summary>
        /// Initiates password reset flow by sending reset email.
        /// Does not reveal whether email exists (security best practice).
        /// </summary>
        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "Email is required",
                        statusCode: 400
                    ));
                }

                await _authService.RequestPasswordResetAsync(request.Email);
                
                _logger.LogInformation($"Password reset email sent (if user exists): {request.Email}");
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { message = "If email exists, password reset link has been sent" },
                    "Password reset email sent",
                    200
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request password reset error");
                throw;
            }
        }

        /// <summary>
        /// Completes password reset using the reset token sent via email.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ResetToken))
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "User ID and reset token are required",
                        statusCode: 400
                    ));
                }

                // Validate new password strength
                var passwordValidation = _passwordValidator.Validate(request.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        $"New password does not meet requirements: {passwordValidation.ErrorMessage}",
                        statusCode: 400
                    ));
                }

                await _authService.ResetPasswordAsync(request.UserId, request.ResetToken, request.NewPassword);
                
                _logger.LogInformation($"Password reset completed for user: {request.UserId}");
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { message = "Password reset successfully" },
                    "Password reset successfully",
                    200
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Password reset failed: {ex.Message}");
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message, statusCode: 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset error");
                throw;
            }
        }

        // ===== USER PROFILE =====

        /// <summary>
        /// Gets the authenticated user's profile information.
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<object>.FailureResponse(
                        "User not found in token",
                        statusCode: 401
                    ));
                }

                var response = await _authService.GetUserProfileAsync(userId);
                if (response == null)
                {
                    return NotFound(ApiResponse<object>.FailureResponse(
                        "User not found",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponse<UserDetailResponse>.SuccessResponse(response, "Profile retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get profile error");
                throw;
            }
        }

        // ===== HELPER METHODS =====

        /// <summary>
        /// Gets the client's IP address from the request.
        /// </summary>
        private string GetClientIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.ToString().Split(',')[0];
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
