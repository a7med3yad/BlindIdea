using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Request payload for resending verification email.
    /// </summary>
    public class ResendVerificationEmailDto
    {
        /// <summary>
        /// The email address that needs verification.
        /// Required, must be valid email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        public string Email { get; set; } = null!;
    }
}
