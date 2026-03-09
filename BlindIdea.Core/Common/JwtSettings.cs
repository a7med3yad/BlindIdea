namespace BlindIdea.Core.Common;

public class JwtSettings
{
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
