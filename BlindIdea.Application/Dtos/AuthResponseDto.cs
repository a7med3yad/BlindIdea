using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Response payload for authentication endpoints (register, login, refresh).
    /// Contains tokens and user information.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Short-lived JWT access token for API authentication (15 minutes).
        /// Include in Authorization header: "Bearer {token}"
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Long-lived refresh token for obtaining new access tokens (7-30 days).
        /// Store securely in HttpOnly cookie or password-protected storage.
        /// Should NOT be passed in Authorization header.
        /// </summary>
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// When the access token expires (UTC timestamp).
        /// Typically 15 minutes from token creation.
        /// </summary>
        public DateTime AccessTokenExpiration { get; set; }

        /// <summary>
        /// When the refresh token expires (UTC timestamp).
        /// Typically 7-30 days from token creation.
        /// </summary>
        public DateTime RefreshTokenExpiration { get; set; }

        /// <summary>
        /// Authenticated user information.
        /// </summary>
        public UserDto User { get; set; } = null!;

        /// <summary>
        /// Legacy property for backward compatibility.
        /// Use AccessTokenExpiration instead.
        /// </summary>
        [Obsolete("Use AccessTokenExpiration instead")]
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Legacy property for backward compatibility.
        /// Use AccessToken instead.
        /// </summary>
        [Obsolete("Use AccessToken instead")]
        public string Token { get; set; } = null!;
    }
}

