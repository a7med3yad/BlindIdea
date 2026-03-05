using System;

namespace BlindIdea.Application.Dtos.Error
{
    /// <summary>
    /// Standard error response returned by API on failure.
    /// Used by global exception middleware for consistent error handling.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP status code (400, 401, 404, 500, etc.).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// High-level error title/type.
        /// E.g., "Validation Error", "Not Found", "Unauthorized"
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed error message explaining what went wrong.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Error code for programmatic handling.
        /// E.g., "ERR_VALIDATION", "ERR_NOT_FOUND"
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Dictionary of validation errors per field.
        /// Key is field name, value is list of error messages.
        /// Only populated for validation errors (400).
        /// </summary>
        public Dictionary<string, List<string>>? Errors { get; set; }

        /// <summary>
        /// Stack trace for debugging (only in development mode).
        /// Should be null in production.
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception details if applicable.
        /// </summary>
        public ErrorResponse? InnerError { get; set; }

        /// <summary>
        /// Timestamp when the error occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request path that caused the error.
        /// Useful for debugging and logging.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Trace ID for tracking request through logs.
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Creates an error response for validation failure.
        /// </summary>
        public static ErrorResponse ValidationError(string message, Dictionary<string, List<string>>? errors = null)
        {
            return new ErrorResponse
            {
                StatusCode = 400,
                Title = "Validation Error",
                Message = message,
                ErrorCode = "ERR_VALIDATION",
                Errors = errors
            };
        }

        /// <summary>
        /// Creates an error response for authentication failure.
        /// </summary>
        public static ErrorResponse Unauthorized(string message)
        {
            return new ErrorResponse
            {
                StatusCode = 401,
                Title = "Unauthorized",
                Message = message,
                ErrorCode = "ERR_UNAUTHORIZED"
            };
        }

        /// <summary>
        /// Creates an error response for authorization failure.
        /// </summary>
        public static ErrorResponse Forbidden(string message)
        {
            return new ErrorResponse
            {
                StatusCode = 403,
                Title = "Forbidden",
                Message = message,
                ErrorCode = "ERR_FORBIDDEN"
            };
        }

        /// <summary>
        /// Creates an error response for not found.
        /// </summary>
        public static ErrorResponse NotFound(string message)
        {
            return new ErrorResponse
            {
                StatusCode = 404,
                Title = "Not Found",
                Message = message,
                ErrorCode = "ERR_NOT_FOUND"
            };
        }

        /// <summary>
        /// Creates an error response for conflict (duplicate).
        /// </summary>
        public static ErrorResponse Conflict(string message)
        {
            return new ErrorResponse
            {
                StatusCode = 409,
                Title = "Conflict",
                Message = message,
                ErrorCode = "ERR_CONFLICT"
            };
        }

        /// <summary>
        /// Creates an error response for server errors.
        /// </summary>
        public static ErrorResponse ServerError(string message, string? stackTrace = null)
        {
            return new ErrorResponse
            {
                StatusCode = 500,
                Title = "Internal Server Error",
                Message = message,
                ErrorCode = "ERR_SERVER_ERROR",
                StackTrace = stackTrace
            };
        }
    }

    /// <summary>
    /// Response for validation errors with field details.
    /// </summary>
    public class ValidationErrorResponse
    {
        /// <summary>
        /// List of validation errors by field.
        /// </summary>
        public Dictionary<string, List<string>> FieldErrors { get; set; } = new();

        /// <summary>
        /// General message about the validation failure.
        /// </summary>
        public string Message { get; set; } = "One or more validation errors occurred";
    }

    /// <summary>
    /// Response for password validation feedback.
    /// </summary>
    public class PasswordValidationErrorResponse
    {
        /// <summary>
        /// List of failed password requirements.
        /// E.g., "Must contain at least one uppercase letter"
        /// </summary>
        public List<string> FailedRequirements { get; set; } = new();

        /// <summary>
        /// General message about password strength.
        /// </summary>
        public string Message { get; set; } = "Password does not meet strength requirements";
    }
}
