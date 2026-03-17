using BlindIdea.API.Core;
using BlindIdea.API.Dtos;
using BlindIdea.API.Entities;
using BlindIdea.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public AuthController(
            UserManager<ApplicationUser> userManager,
            OtpService otpService,
            TokenService tokenService,
            EmailService emailService,
            AppDbContext context)
        {
            _userManager = userManager;
            _otpService = otpService;
            _tokenService = tokenService;
            _emailService = emailService;
            _context = context;
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok("Authorized");
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

            user.IsVerified= true;

            user.OtpExpiration = null;

            await _userManager.UpdateAsync(user);

            await _userManager.RemoveAuthenticationTokenAsync(user, "AuthApi", "OTP");

            return Ok(new AuthResponseDto
            {
                AccessToken = _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> login(RegisterDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user==null) return Unauthorized("User not register");
            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return Unauthorized("Invalid password");

            return Ok(new AuthResponseDto
            {
                AccessToken = _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> forgetpassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();
            var otp = _otpService.GenerateOtp();
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
                AccessToken = _tokenService.CreateAccessToken(storedToken.User),
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
                AccessToken = _tokenService.CreateAccessToken(user),
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



    }
}
