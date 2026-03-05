using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Request payload for user login.
    /// Both email and password are required.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// User's email address.
        /// Required, must be valid email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// User's password.
        /// Required, must match the registered password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 1, ErrorMessage = "Password cannot exceed 256 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Optional: Remember this login
        /// If true, refresh token expiry will be longer (30 days).
        /// If false, refresh token expiry is standard (7 days).
        /// </summary>
        public bool RememberMe { get; set; }
    }

}
