using System;
using System.Collections.Generic;
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
        /// </summary>
        public string Email { get; set; } = null!;
    }
}
