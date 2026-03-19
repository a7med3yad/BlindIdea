using BlindIdea.API.Core;
using BlindIdea.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using BlindIdea.API.Dtos.Auth;
using BlindIdea.API.Services.Auth;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OtpService _otpService;
        private readonly TokenService _tokenService;
        private readonly EmailService _emailService;
        private readonly AppDbContext _context;
        private readonly OAuthService _oAuthService;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthController(
            UserManager<ApplicationUser> userManager,
            OtpService otpService,
            TokenService tokenService,
            EmailService emailService,
            AppDbContext context,
            RoleManager<IdentityRole> roleManager,
            OAuthService oAuthService
            )
        {
            _userManager = userManager;
            _otpService = otpService;
            _tokenService = tokenService;
            _emailService = emailService;
            _context = context;
            _roleManager = roleManager;
            _oAuthService = oAuthService;
        }
        // ✅ Only Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-panel")]
        public IActionResult AdminPanel()
        {
            return Ok("Welcome Admin");
        }

        // ✅ Only User
        [Authorize(Roles = "User")]
        [HttpGet("user-dashboard")]
        public IActionResult UserDashboard()
        {
            return Ok("Welcome User");
        }

        // ✅ Both Admin and User
        [Authorize(Roles = "Admin,User")]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new { email, role });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return BadRequest("User not found");

            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest("Role does not exist");

            await _userManager.AddToRoleAsync(user, role);

            return Ok($"Role {role} assigned to {email}");
        }

        [HttpPost("register")]
        public async Task<IActionResult> register(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await _userManager.AddToRoleAsync(user, "User");

            if (!_otpService.CanRequestOtp(user))
            {
                return BadRequest("Too many OTP requests. Please wait 10 minutes.");
            }

            var otp = _otpService.GenerateOtp();

            user.OtpExpiration = _otpService.GetExpiration();

            await _emailService.SendOtp(user.Email, otp);

            await _userManager.SetAuthenticationTokenAsync(
                user, "AuthApi", "OTP", otp
            );
            return Ok("OTP sent to email. Valid for 5 minuts.");

        }
        [HttpPost("Verify-email")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("User not found");

            if (_otpService.IsExpired(user.OtpExpiration))
                return BadRequest("OTP has expired. Please request a new one.");
            var storedOtp = await _userManager.GetAuthenticationTokenAsync(
                user, "AuthApi", "OTP"
            );

            if (storedOtp != dto.Otp)
            {
                return BadRequest("Invalid OTP");
            }

            user.IsVerified = true;

            user.OtpExpiration = null;

            await _userManager.UpdateAsync(user);

            await _userManager.RemoveAuthenticationTokenAsync(user, "AuthApi", "OTP");

            return Ok(new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> login(RegisterDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("User not register");
            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return Unauthorized("Invalid password");

            return Ok(new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> forgetpassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest("User not found");


            if (!_otpService.CanRequestOtp(user))
                return BadRequest("Too many OTP requests. Please wait 10 minutes.");

            var otp = _otpService.GenerateOtp();
            user.OtpExpiration = _otpService.GetExpiration();
            await _userManager.UpdateAsync(user);
            await _userManager.SetAuthenticationTokenAsync(
                user, "AuthApi", "ResetOTP", otp
            );
            await _emailService.SendOtp(email, otp);
            return Ok("OTP Sent");
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);

            if (storedToken == null) return Unauthorized("Invalid refresh token");
            if (!storedToken.IsActive)
                return Unauthorized("Refresh token expired or revoked. Please login again.");
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
            return Ok(new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(storedToken.User),
                RefreshToken = (await _tokenService.CreateRefreshToken(storedToken.User)).Token
            });
        }
        [HttpPost("verify-reset")]
        public async Task<IActionResult> VerifyReset(VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            var otp = await _userManager.GetAuthenticationTokenAsync(
                user, "AuthApi", "ResetOTP"
            );

            if (otp != dto.Otp)
                return BadRequest("Invalid OTP");

            return Ok(new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return Unauthorized();
            var result = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword
            );
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok("Password changed successfully");
        }
        // ✅ Google Login
        [HttpGet("login/google")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("ExternalCallback", "Auth");
            var props = new AuthenticationProperties
            {
                RedirectUri = redirectUrl  // ✅ points to ExternalCallback
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // ✅ GitHub Login
        [HttpGet("login/github")]
        public IActionResult GitHubLogin()
        {
            var redirectUrl = Url.Action("ExternalCallback", "Auth");
            var props = new AuthenticationProperties
            {
                RedirectUri = redirectUrl  // ✅ same ExternalCallback for both
            };
            return Challenge(props, GitHubAuthenticationDefaults.AuthenticationScheme);
        }

        // ✅ One unified callback for both Google and GitHub
        // Middleware handles /signin-google and /signin-github automatically
        // then redirects here
        [HttpGet("external-callback")]
        public async Task<IActionResult> ExternalCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return Unauthorized($"External login failed: {result.Failure?.Message}");

            try
            {
                var response = await _oAuthService.HandleOAuthLogin(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
