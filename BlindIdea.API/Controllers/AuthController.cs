using BlindIdea.Application.Dtos.Auth.Requests;
using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlindIdea.API.Controllers;

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
    /// Register a new user account. Sends a verification email on success.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Registration successful. Please verify your email.", 201));
    }

    /// <summary>
    /// Login with email/username and password.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result == null)
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid email/username or password", statusCode: 401));

        return Ok(ApiResponse<object>.SuccessResponse(result, "Login successful"));
    }

    /// <summary>
    /// Refresh an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        if (result == null)
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid or expired refresh token", statusCode: 401));

        return Ok(ApiResponse<object>.SuccessResponse(result, "Token refreshed successfully"));
    }

    /// <summary>
    /// Verify email via clickable link (GET) or API call (POST).
    /// GET: /api/auth/verify-email?userId={id}&amp;token={token}
    /// </summary>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> VerifyEmailGet(
        [FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(
            new VerifyEmailRequest { UserId = userId, Token = token }, cancellationToken);

        if (!result)
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid or expired verification link"));

        return Ok(ApiResponse<object>.SuccessResponse(new { verified = true }, "Email verified successfully"));
    }

    /// <summary>
    /// Verify email via POST request body.
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> VerifyEmailPost(
        [FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(request, cancellationToken);

        if (!result)
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid or expired verification token"));

        return Ok(ApiResponse<object>.SuccessResponse(new { verified = true }, "Email verified successfully"));
    }

    /// <summary>
    /// Resend the verification email if the user has not yet verified.
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendVerificationEmailAsync(request, cancellationToken);
        if (!result)
            return BadRequest(ApiResponse<object>.FailureResponse("User not found or email already verified"));

        return Ok(ApiResponse<object>.SuccessResponse(new { sent = true }, "Verification email sent"));
    }
}
