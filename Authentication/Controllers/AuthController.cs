using AspNet.Security.OAuth.GitHub;
using BlindIdea.Application.Dtos.Auth;
using BlindIdea.Application.Services.Abstraction;
using BlindIdea.Application.Services.Abstraction.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        // ✅ Register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var message = await _authService.RegisterAsync(dto);
                return Ok(message);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Verify Email
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyOtpDto dto)
        {
            try
            {
                var response = await _authService.VerifyEmailAsync(dto);
                return Ok(response);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(RegisterDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (Exception ex) { return Unauthorized(ex.Message); }
        }

        // ✅ Forgot Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                var message = await _authService.ForgotPasswordAsync(email);
                return Ok(message);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Verify Reset
        [HttpPost("verify-reset")]
        public async Task<IActionResult> VerifyReset(VerifyOtpDto dto)
        {
            try
            {
                var response = await _authService.VerifyResetAsync(dto);
                return Ok(response);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Refresh Token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(dto);
                return Ok(response);
            }
            catch (Exception ex) { return Unauthorized(ex.Message); }
        }

        // ✅ Logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenDto dto)
        {
            try
            {
                await _authService.LogoutAsync(dto);
                return Ok("Logged out successfully");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Change Password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            try
            {
                await _authService.ChangePasswordAsync(GetUserId(), dto);
                return Ok("Password changed successfully");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Assign Role
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string email, string role)
        {
            try
            {
                await _authService.AssignRoleAsync(email, role);
                return Ok($"Role {role} assigned to {email}");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // ✅ Profile
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new { email, role });
        }

        // ✅ Google Login
        [HttpGet("login/google")]
        public IActionResult GoogleLogin(string? returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalCallback", "Auth"),
                Items = { { "returnUrl", returnUrl ?? "" } }
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // ✅ GitHub Login
        [HttpGet("login/github")]
        public IActionResult GitHubLogin(string? returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalCallback", "Auth"),
                Items = { { "returnUrl", returnUrl ?? "" } }
            };
            return Challenge(props, GitHubAuthenticationDefaults.AuthenticationScheme);
        }

        // ✅ OAuth Callback
        [HttpGet("external-callback")]
        public async Task<IActionResult> ExternalCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return Unauthorized($"External login failed: {result.Failure?.Message}");

            var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value
                ?? result.Principal?.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest("Email not provided by OAuth provider");

            try
            {
                var response = await _authService.HandleOAuthLoginAsync(email);

                // Read the returnUrl stored during login initiation
                var returnUrl = result.Properties?.Items.ContainsKey("returnUrl") == true
                    ? result.Properties.Items["returnUrl"]
                    : null;

                // Build the frontend callback URL with tokens
                if (string.IsNullOrEmpty(returnUrl))
                    returnUrl = "http://localhost:3000/external-callback";

                var separator = returnUrl.Contains('?') ? '&' : '?';
                var redirectUrl = $"{returnUrl}{separator}accessToken={Uri.EscapeDataString(response.AccessToken)}&refreshToken={Uri.EscapeDataString(response.RefreshToken)}";

                // Clean up external cookie
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                return Redirect(redirectUrl);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}