using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Infrastructure.Common.Options
{
    /// <summary>
    /// Configuration options for JWT token generation and validation.
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Secret key used to sign JWT tokens. Should be at least 32 characters.
        /// Loaded from environment variable: JWT_SECRET or appsettings.json
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// JWT issuer claim. Identifies the principal that issued the token.
        /// Loaded from environment variable: JWT_ISSUER or appsettings.json
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// JWT audience claim. Identifies the recipients that the token is intended for.
        /// Loaded from environment variable: JWT_AUDIENCE or appsettings.json
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in minutes (default: 15 minutes).
        /// Loaded from environment variable: JWT_ACCESS_EXPIRY_MINUTES or appsettings.json
        /// </summary>
        public int AccessTokenExpiryMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh token expiration time in days (default: 7 days).
        /// Loaded from environment variable: JWT_REFRESH_EXPIRY_DAYS or appsettings.json
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; } = 7;

        /// <summary>
        /// Legacy property for backward compatibility. Use AccessTokenExpiryMinutes instead.
        /// </summary>
        [Obsolete("Use AccessTokenExpiryMinutes instead")]
        public int ExpireDays { get; set; }
    }

}
