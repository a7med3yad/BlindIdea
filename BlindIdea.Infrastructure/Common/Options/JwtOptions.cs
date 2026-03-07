namespace BlindIdea.Infrastructure.Common.Options
{
    /// <summary>
    /// JWT configuration options
    /// </summary>
    public class JwtOptions
    {
        public string? Secret { get; set; }

        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public int AccessTokenExpiryMinutes { get; set; } = 15;

        public int RefreshTokenExpiryDays { get; set; } = 7;

        public int EmailVerificationTokenExpiryMinutes { get; set; } = 60;
    }
}
