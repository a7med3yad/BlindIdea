namespace BlindIdea.Infrastructure.Common.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string? Secret { get; set; }
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public int EmailVerificationTokenExpiryMinutes { get; set; } = 60;

    public string ResolvedSecret => Secret ?? Key ?? throw new InvalidOperationException("JWT Secret/Key is not configured");
}
