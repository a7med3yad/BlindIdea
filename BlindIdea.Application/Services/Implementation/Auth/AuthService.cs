using BlindIdea.Application.Dtos.Auth;
using BlindIdea.Application.Services.Abstraction.Auth;
using BlindIdea.Domain.Abstraction.Services;
using BlindIdea.Domain.Entities;
using BlindIdea.Infrastructure.Implementation.Auth;
using BlindIdea.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindIdea.Application.Implementation.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly AppDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService,
            IOtpService otpService,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _otpService = otpService;
            _context = context;
        }

        // ✅ Register
        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already registered");

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "User");

            if (!_otpService.CanRequestOtp(user))
                throw new Exception("Too many OTP requests. Please wait 10 minutes.");

            var otp = _otpService.GenerateOtp();
            user.OtpExpiration = _otpService.GetExpiration();

            await _userManager.UpdateAsync(user);
            await _userManager.SetAuthenticationTokenAsync(user, "AuthApi", "OTP", otp);
            await _emailService.SendOtp(user.Email!, otp);

            return "OTP sent to email. Valid for 5 minutes.";
        }

        // ✅ Verify Email OTP
        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new Exception("User not found");

            if (_otpService.IsExpired(user.OtpExpiration))
                throw new Exception("OTP has expired. Please request a new one.");

            var storedOtp = await _userManager.GetAuthenticationTokenAsync(user, "AuthApi", "OTP");

            if (storedOtp != dto.Otp)
                throw new Exception("Invalid OTP");

            user.IsVerified = true;
            user.OtpExpiration = null;
            await _userManager.UpdateAsync(user);
            await _userManager.RemoveAuthenticationTokenAsync(user, "AuthApi", "OTP");

            return new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            };
        }

        // ✅ Login
        public async Task<AuthResponseDto> LoginAsync(RegisterDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new Exception("User not registered");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid password");
            if(user.IsVerified == false)
                throw new Exception("Email not verified. Please verify your email before logging in.");
            return new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            };
        }

        // ✅ Forgot Password
        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("User not found");

            if (!_otpService.CanRequestOtp(user))
                throw new Exception("Too many OTP requests. Please wait 10 minutes.");

            var otp = _otpService.GenerateOtp();
            user.OtpExpiration = _otpService.GetExpiration();
            await _userManager.UpdateAsync(user);
            await _userManager.SetAuthenticationTokenAsync(user, "AuthApi", "ResetOTP", otp);
            await _emailService.SendOtp(email, otp);

            return "OTP sent. Valid for 5 minutes.";
        }

        // ✅ Verify Reset OTP
        public async Task<AuthResponseDto> VerifyResetAsync(VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new Exception("User not found");

            if (_otpService.IsExpired(user.OtpExpiration))
                throw new Exception("OTP has expired. Please request a new one.");

            var storedOtp = await _userManager.GetAuthenticationTokenAsync(user, "AuthApi", "ResetOTP");

            if (storedOtp != dto.Otp)
                throw new Exception("Invalid OTP");

            user.OtpExpiration = null;
            await _userManager.UpdateAsync(user);
            await _userManager.RemoveAuthenticationTokenAsync(user, "AuthApi", "ResetOTP");

            return new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            };
        }

        // ✅ Refresh Token
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken)
                ?? throw new Exception("Invalid refresh token");

            if (!storedToken.IsActive)
                throw new Exception("Refresh token expired or revoked. Please login again.");

            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(storedToken.User),
                RefreshToken = (await _tokenService.CreateRefreshToken(storedToken.User)).Token
            };
        }

        // ✅ Logout
        public async Task LogoutAsync(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken)
                ?? throw new Exception("Invalid token");

            if (!storedToken.IsActive)
                throw new Exception("Token already revoked");

            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        // ✅ Change Password
        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // ✅ OAuth Login — called from ExternalCallback
        public async Task<AuthResponseDto> HandleOAuthLoginAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    IsVerified = true
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                    throw new Exception($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                await _userManager.AddToRoleAsync(user, "User");

                // ✅ Reload fresh from DB
                user = await _userManager.FindByEmailAsync(email)
                    ?? throw new Exception("User not found after creation");
            }

            return new AuthResponseDto
            {
                AccessToken = await _tokenService.CreateAccessToken(user),
                RefreshToken = (await _tokenService.CreateRefreshToken(user)).Token
            };
        }

        // ✅ Assign Role — admin only
        public async Task AssignRoleAsync(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("User not found");

            if (!await _roleManager.RoleExistsAsync(role))
                throw new Exception("Role does not exist");

            await _userManager.AddToRoleAsync(user, role);
        }
    }
}