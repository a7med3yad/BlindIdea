using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Request payload for user registration.
    /// All fields are required and validated.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// User's full name.
        /// Required, between 2-100 characters.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// User's email address.
        /// Required, must be valid email format, maximum 255 characters.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is not in a valid format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// User's password.
        /// Required. Must meet strength requirements:
        /// - Minimum 8 characters
        /// - At least one uppercase letter (A-Z)
        /// - At least one lowercase letter (a-z)
        /// - At least one digit (0-9)
        /// - At least one special character (!@#$%^&*_+=-[]{}';:"\\|,.<>/?")
        /// Password validation is performed by IPasswordValidator service.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 256 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Password confirmation.
        /// Must match Password field exactly.
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }

}
