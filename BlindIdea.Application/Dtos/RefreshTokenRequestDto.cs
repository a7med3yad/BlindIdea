using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Request payload for token refresh endpoint.
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// The refresh token obtained from previous login/registration.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
    }
}
