using BlindIdea.Application.Dtos.Auth.Requests;
using BlindIdea.Application.Dtos.Auth.Responses;
using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Common;
using BlindIdea.Core.Entities;
using BlindIdea.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace BlindIdea.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailSender emailSender,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailSender = emailSender;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists.");

        existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
            throw new InvalidOperationException("User with this username already exists.");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // FIX: URL-encode the token so it survives being embedded in a query string link
        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        try
        {
            await _emailSender.SendVerificationEmailAsync(user.Id!, user.Email!, encodedToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification email to {Email}", user.Email);
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.EmailOrUserName)
            ?? await _userManager.FindByNameAsync(request.EmailOrUserName);

        if (user == null || user.IsDeleted) return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded) return null;

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        // FIX: also filter out expired tokens at query level
        var tokens = await _unitOfWork.RefreshTokens.FindAsync(rt =>
            rt.Token == request.RefreshToken && !rt.IsRevoked);

        var refreshTokenEntity = tokens.FirstOrDefault();

        if (refreshTokenEntity == null || refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await _unitOfWork.Users.GetByIdAsync(refreshTokenEntity.UserId);
        if (user == null || user.IsDeleted) return null;

        // Rotate: revoke old, issue new
        refreshTokenEntity.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(refreshTokenEntity);

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id!,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            User = MapToUserResponse(user)
        };
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null) return false;

        // FIX: decode the URL-safe Base64 token back to the original token string
        string originalToken;
        try
        {
            originalToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch
        {
            // Token was not encoded (e.g. called directly via POST with raw token)
            originalToken = request.Token;
        }

        var result = await _userManager.ConfirmEmailAsync(user, originalToken);
        if (!result.Succeeded) return false;

        user.EmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // FIX: also guard against deleted users
        if (user == null || user.IsDeleted || user.EmailVerified) return false;

        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        await _emailSender.SendVerificationEmailAsync(user.Id!, user.Email!, encodedToken, cancellationToken);
        return true;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, CancellationToken ct = default)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id!,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            User = MapToUserResponse(user)
        };
    }

    private static UserResponse MapToUserResponse(User user) => new()
    {
        Id = user.Id!,
        Email = user.Email ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        EmailVerified = user.EmailVerified
    };
}
