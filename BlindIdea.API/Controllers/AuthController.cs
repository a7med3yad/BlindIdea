using BlindIdea.Application.Dtos.Auth.Requests;
using BlindIdea.Application.Dtos.Common;
using BlindIdea.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

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

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        if (result == null)
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid or expired refresh token", statusCode: 401));
        return Ok(ApiResponse<object>.SuccessResponse(result, "Token refreshed"));
    }

    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(new VerifyEmailRequest { Token = token, Email = email }, cancellationToken);
        if (!result)
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid or expired verification link"));
        return Ok(ApiResponse<object>.SuccessResponse(new { verified = true }, "Email verified successfully"));
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendVerificationEmailAsync(request, cancellationToken);
        if (!result)
            return BadRequest(ApiResponse<object>.FailureResponse("User not found or email already verified"));
        return Ok(ApiResponse<object>.SuccessResponse(new { sent = true }, "Verification email sent"));
    }
}
