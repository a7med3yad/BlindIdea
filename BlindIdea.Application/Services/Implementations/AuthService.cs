using BlindIdea.Infrastructure.Common.Options;
using BlindIdea.Application.Dtos;
using BlindIdea.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Interfaces;
using BlindIdea.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BlindIdea.Application.Services.Implementations
{
    /// <summary>
    /// Service for user authentication, registration, login, token management, and email verification.
    /// Implements production-grade security practices including token rotation, email verification,
    /// and comprehensive audit logging.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _dbContext;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<AuthService> _logger;

        // Email verification token expiry: 24 hours
        private const int EmailVerificationTokenExpiryHours = 24;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IEmailService emailService,
            AppDbContext dbContext,
            IOptions<JwtOptions> jwtOptions,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _dbContext = dbContext;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with email and password.
        /// Sends email verification link and returns tokens.
        /// </summary>
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("Registration attempted with empty email or password");
                    return null;
                }

                // 2. Check if user exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration attempted with existing email: {dto.Email}");
                    return null;
                }

                // 3. Create new user
                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    UserName = dto.Email,
                    IsDeleted = false,
                    EmailConfirmed = false // Start as unverified
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"User creation failed for email: {dto.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return null;
                }

                _logger.LogInformation($"User registered successfully: {user.Id} ({dto.Email})");

                // 4. Generate verification token and send email
                await SendVerificationEmailAsync(user);

                // 5. Generate tokens
                var accessToken = _jwtService.CreateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var jwtId = _jwtService.ExtractJwtId(accessToken) ?? Guid.NewGuid().ToString();

                // 6. Store refresh token in database
                await StoreRefreshTokenAsync(user.Id, refreshToken, jwtId, null);

                _logger.LogInformation($"Tokens generated and stored for user: {user.Id}");

                // 7. Return response
                var accessTokenExpiryMinutes = _jwtOptions.AccessTokenExpiryMinutes > 0
                    ? _jwtOptions.AccessTokenExpiryMinutes : 15;
                var refreshTokenExpiryDays = _jwtOptions.RefreshTokenExpiryDays > 0
                    ? _jwtOptions.RefreshTokenExpiryDays : 7;

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
                    RefreshTokenExpiration = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email!,
                    },
                    // Backward compatibility
                    Token = accessToken,
                    Expiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during registration: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Authenticates user with email and password.
        /// Returns access and refresh tokens if credentials are valid.
        /// </summary>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, string ipAddress)
        {
            try
            {
                // 1. Find user
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning($"Login attempt with non-existent email: {dto.Email} from IP: {ipAddress}");
                    return null;
                }

                // 2. Check if email is verified
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning($"Login attempt with unverified email: {dto.Email} from IP: {ipAddress}");
                    return null;
                }

                // 3. Validate password
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Failed login attempt for user: {user.Id} from IP: {ipAddress}");
                    return null;
                }

                _logger.LogInformation($"User logged in successfully: {user.Id} from IP: {ipAddress}");

                // 4. Generate tokens
                var accessToken = _jwtService.CreateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var jwtId = _jwtService.ExtractJwtId(accessToken) ?? Guid.NewGuid().ToString();

                // 5. Store refresh token
                await StoreRefreshTokenAsync(user.Id, refreshToken, jwtId, ipAddress);

                // 6. Return response
                var accessTokenExpiryMinutes = _jwtOptions.AccessTokenExpiryMinutes > 0
                    ? _jwtOptions.AccessTokenExpiryMinutes : 15;
                var refreshTokenExpiryDays = _jwtOptions.RefreshTokenExpiryDays > 0
                    ? _jwtOptions.RefreshTokenExpiryDays : 7;

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
                    RefreshTokenExpiration = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email!,
                    },
                    // Backward compatibility
                    Token = accessToken,
                    Expiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during login: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Implements token rotation: revokes old token and issues new one.
        /// Detects reuse of revoked tokens (security breach indicator).
        /// </summary>
        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("Refresh token request with empty token");
                    return null;
                }

                // 1. Hash the token to find it in database
                var tokenHash = _jwtService.HashToken(refreshToken);

                // 2. Find the refresh token record
                var storedToken = _dbContext.RefreshTokens
                    .FirstOrDefault(rt => rt.TokenHash == tokenHash);

                if (storedToken == null)
                {
                    _logger.LogWarning($"Refresh token not found in database");
                    return null;
                }

                // 3. Validate token
                if (!storedToken.IsValid)
                {
                    // If token is revoked and reused, it's a security breach
                    if (storedToken.IsRevoked && storedToken.IsUsed)
                    {
                        _logger.LogError($"SECURITY ALERT: Revoked token reused by user {storedToken.UserId} from IP {ipAddress}. Revoking all tokens.");
                        // Revoke entire token family (all tokens issued to user)
                        await RevokeAllTokensAsync(storedToken.UserId, ipAddress);
                        return null;
                    }

                    _logger.LogWarning($"Invalid refresh token for user: {storedToken.UserId}. Valid: {storedToken.IsValid}, Expired: {storedToken.IsExpired}, Revoked: {storedToken.IsRevoked}");
                    return null;
                }

                // 4. Get user
                var user = await _userManager.FindByIdAsync(storedToken.UserId);
                if (user == null || user.IsDeleted || !user.EmailConfirmed)
                {
                    _logger.LogWarning($"User not found or inactive for refresh: {storedToken.UserId}");
                    return null;
                }

                // 5. Mark old token as used (implements single-use pattern)
                storedToken.IsUsed = true;
                _dbContext.RefreshTokens.Update(storedToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Refresh token marked as used for user: {user.Id}");

                // 6. Generate new tokens
                var newAccessToken = _jwtService.CreateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newJwtId = _jwtService.ExtractJwtId(newAccessToken) ?? Guid.NewGuid().ToString();

                // 7. Store new refresh token and link to old one (token family tracking)
                var newStoredToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = _jwtService.HashToken(newRefreshToken),
                    JwtId = newJwtId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays > 0 ? _jwtOptions.RefreshTokenExpiryDays : 7),
                    IsUsed = false
                };

                // Link new token to old one for audit trail
                storedToken.ReplacedByTokenId = newStoredToken.Id;

                _dbContext.RefreshTokens.Add(newStoredToken);
                _dbContext.RefreshTokens.Update(storedToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"New tokens generated for user: {user.Id} (Token rotation)");

                // 8. Return response
                var accessTokenExpiryMinutes = _jwtOptions.AccessTokenExpiryMinutes > 0
                    ? _jwtOptions.AccessTokenExpiryMinutes : 15;
                var refreshTokenExpiryDays = _jwtOptions.RefreshTokenExpiryDays > 0
                    ? _jwtOptions.RefreshTokenExpiryDays : 7;

                return new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
                    RefreshTokenExpiration = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email!,
                    },
                    // Backward compatibility
                    Token = newAccessToken,
                    Expiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during token refresh: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Logs out user by revoking their current refresh token.
        /// </summary>
        public async Task<bool> LogoutAsync(string refreshToken, string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return false;

                var tokenHash = _jwtService.HashToken(refreshToken);
                var storedToken = _dbContext.RefreshTokens
                    .FirstOrDefault(rt => rt.TokenHash == tokenHash);

                if (storedToken == null)
                    return false;

                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;

                _dbContext.RefreshTokens.Update(storedToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"User logged out: {storedToken.UserId} from IP: {ipAddress}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during logout: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Revokes ALL refresh tokens for a user (logout from all devices).
        /// Used for security breach recovery or account reset.
        /// </summary>
        public async Task<bool> RevokeAllTokensAsync(string userId, string ipAddress)
        {
            try
            {
                var tokens = _dbContext.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToList();

                if (!tokens.Any())
                    return true; // No active tokens to revoke

                foreach (var token in tokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                }

                _dbContext.RefreshTokens.UpdateRange(tokens);
                await _dbContext.SaveChangesAsync();

                _logger.LogWarning($"All refresh tokens revoked for user: {userId} from IP: {ipAddress}. Token count: {tokens.Count}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during token revocation: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifies email address using a verification token.
        /// Marks EmailConfirmed=true on successful verification.
        /// </summary>
        public async Task<bool> VerifyEmailAsync(string userId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Email verification attempted with empty userId or token");
                    return false;
                }

                // 1. Find user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Email verification attempted with non-existent user: {userId}");
                    return false;
                }

                // 2. Check if already verified
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation($"Email already verified for user: {userId}");
                    return true;
                }

                // 3. Hash the token
                var tokenHash = _jwtService.HashToken(token);

                // 4. Find verification token
                var verificationToken = _dbContext.EmailVerificationTokens
                    .FirstOrDefault(evt => evt.UserId == userId && evt.TokenHash == tokenHash);

                if (verificationToken == null)
                {
                    _logger.LogWarning($"Email verification token not found for user: {userId}");
                    return false;
                }

                // 5. Validate token
                if (verificationToken.IsExpired || verificationToken.IsUsed)
                {
                    _logger.LogWarning($"Email verification token invalid for user: {userId}. Expired: {verificationToken.IsExpired}, Used: {verificationToken.IsUsed}");
                    return false;
                }

                // 6. Mark email as confirmed
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                // 7. Mark token as used
                verificationToken.VerifiedAt = DateTime.UtcNow;
                _dbContext.EmailVerificationTokens.Update(verificationToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Email verified successfully for user: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during email verification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resends email verification token to unverified users.
        /// Rate-limited: max once per 2 minutes per user.
        /// </summary>
        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Resend verification attempted with empty email");
                    return false;
                }

                // 1. Find user
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"Resend verification attempted with non-existent email: {email}");
                    return false;
                }

                // 2. Check if email already verified
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation($"Resend verification attempted for already verified email: {email}");
                    return true;
                }

                // 3. Check rate limit (not sent in last 2 minutes)
                var recentToken = _dbContext.EmailVerificationTokens
                    .Where(evt => evt.UserId == user.Id)
                    .OrderByDescending(evt => evt.CreatedAt)
                    .FirstOrDefault();

                if (recentToken != null && (DateTime.UtcNow - recentToken.CreatedAt).TotalMinutes < 2)
                {
                    _logger.LogWarning($"Resend verification rate limit exceeded for user: {user.Id}");
                    return false; // Rate limited
                }

                // 4. Delete old unused tokens
                var oldTokens = _dbContext.EmailVerificationTokens
                    .Where(evt => evt.UserId == user.Id && !evt.IsUsed)
                    .ToList();

                _dbContext.EmailVerificationTokens.RemoveRange(oldTokens);
                await _dbContext.SaveChangesAsync();

                // 5. Send new verification email
                await SendVerificationEmailAsync(user);

                _logger.LogInformation($"Verification email resent to user: {user.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during resend verification: {ex.Message}");
                return false;
            }
        }

        // ============ HELPER METHODS ============

        /// <summary>
        /// Stores a refresh token in the database with hashing.
        /// </summary>
        private async Task StoreRefreshTokenAsync(string userId, string refreshToken, string jwtId, string? ipAddress)
        {
            try
            {
                var expiryDays = _jwtOptions.RefreshTokenExpiryDays > 0
                    ? _jwtOptions.RefreshTokenExpiryDays : 7;

                var storedToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = _jwtService.HashToken(refreshToken),
                    JwtId = jwtId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                    ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                    IsUsed = false
                };

                _dbContext.RefreshTokens.Add(storedToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Refresh token stored for user: {userId}. Expires: {storedToken.ExpiresAt}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception storing refresh token: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generates and sends email verification token to user.
        /// </summary>
        private async Task SendVerificationEmailAsync(User user)
        {
            try
            {
                // 1. Generate verification token
                var token = _jwtService.GenerateRefreshToken();
                var tokenHash = _jwtService.HashToken(token);

                // 2. Store token in database
                var verificationToken = new EmailVerificationToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = tokenHash,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(EmailVerificationTokenExpiryHours)
                };

                _dbContext.EmailVerificationTokens.Add(verificationToken);
                await _dbContext.SaveChangesAsync();

                // 3. Build verification link
                // TODO: Configure APP_BASE_URL in appsettings or environment
                var baseUrl = "https://localhost:7001"; // Replace with configuration
                var verificationLink = $"{baseUrl}/auth/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // 4. Send email
                var subject = "Email Verification - BlindIdea";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Welcome to BlindIdea!</h2>
                        <p>Thank you for registering. Please verify your email address to activate your account.</p>
                        <p>
                            <a href='{verificationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Verify Email
                            </a>
                        </p>
                        <p>Or copy and paste this link in your browser:</p>
                        <p><code>{verificationLink}</code></p>
                        <p style='color: #666; font-size: 12px;'>This link expires in {EmailVerificationTokenExpiryHours} hours.</p>
                        <p style='color: #999; font-size: 12px;'>If you didn't register for BlindIdea, please ignore this email.</p>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(user.Email!, subject, htmlBody);

                _logger.LogInformation($"Verification email sent to user: {user.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception sending verification email: {ex.Message}");
                throw;
            }
        }
    }
}
