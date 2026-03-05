using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Interfaces
{
    /// <summary>
    /// Service for validating password strength and quality.
    /// Enforces security requirements for user passwords.
    /// </summary>
    public interface IPasswordValidator
    {
        /// <summary>
        /// Validates password strength according to security requirements.
        /// </summary>
        /// <remarks>
        /// Password must meet all of the following requirements:
        /// - Minimum 8 characters long
        /// - At least one uppercase letter (A-Z)
        /// - At least one lowercase letter (a-z)
        /// - At least one digit (0-9)
        /// - At least one special character (!@#$%^&*_+=)
        /// </remarks>
        /// <param name="password">The password to validate</param>
        /// <returns>ValidationResult with IsValid flag and error message if invalid</returns>
        PasswordValidationResult Validate(string password);
    }

    /// <summary>
    /// Result of password validation.
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// Indicates if the password meets all strength requirements.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Details about validation failure (null if valid).
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// List of specific requirements that failed.
        /// </summary>
        public List<string> FailedRequirements { get; set; } = new();

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static PasswordValidationResult Success()
        {
            return new PasswordValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        public static PasswordValidationResult Failure(List<string> failedRequirements)
        {
            var requirements = failedRequirements ?? new();
            return new PasswordValidationResult
            {
                IsValid = false,
                FailedRequirements = requirements,
                ErrorMessage = "Password does not meet security requirements: " + string.Join(", ", requirements)
            };
        }
    }
}
