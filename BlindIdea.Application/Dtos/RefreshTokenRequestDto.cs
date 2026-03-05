using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        /// Required for token refresh.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        [StringLength(500, ErrorMessage = "Refresh token cannot exceed 500 characters")]
        public string RefreshToken { get; set; } = null!;
    }
}
