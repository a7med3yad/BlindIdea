using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
    /// <summary>
    /// Standard error response for API endpoints.
    /// Used for validation errors, authentication failures, and other error conditions.
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// HTTP status code of the error.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Brief error title.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed error message.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Validation errors if applicable (null for non-validation errors).
        /// </summary>
        public Dictionary<string, List<string>>? ValidationErrors { get; set; }

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response for validation errors specifically.
    /// Contains detailed per-field error information.
    /// </summary>
    public class ValidationErrorResponseDto
    {
        /// <summary>
        /// HTTP status code (typically 400).
        /// </summary>
        public int StatusCode { get; set; } = 400;

        /// <summary>
        /// Title for validation error.
        /// </summary>
        public string Title { get; set; } = "Validation Failed";

        /// <summary>
        /// General error message.
        /// </summary>
        public string Message { get; set; } = "One or more validation errors occurred.";

        /// <summary>
        /// Dictionary of field names to list of error messages for that field.
        /// Example: { "password": ["At least 8 characters", "At least one uppercase"] }
        /// </summary>
        public Dictionary<string, List<string>> Errors { get; set; } = new();

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response for password validation errors specifically.
    /// Provides detailed password strength requirements.
    /// </summary>
    public class PasswordValidationErrorDto
    {
        /// <summary>
        /// HTTP status code (typically 400).
        /// </summary>
        public int StatusCode { get; set; } = 400;

        /// <summary>
        /// Error title.
        /// </summary>
        public string Title { get; set; } = "Password Validation Failed";

        /// <summary>
        /// General error message.
        /// </summary>
        public string Message { get; set; } = "Password does not meet security requirements.";

        /// <summary>
        /// List of password requirements that were not met.
        /// </summary>
        public List<string> FailedRequirements { get; set; } = new();

        /// <summary>
        /// Password requirements that must be met.
        /// </summary>
        public List<string> RequiredRequirements { get; set; } = new()
        {
            "At least 8 characters",
            "At least one uppercase letter (A-Z)",
            "At least one lowercase letter (a-z)",
            "At least one number (0-9)",
            "At least one special character (!@#$%^&*_+=-[]{}';:\"\\|,.<>/?)"
        };

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
