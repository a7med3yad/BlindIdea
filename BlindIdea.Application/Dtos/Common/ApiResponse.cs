using System;

namespace BlindIdea.Application.Dtos.Common
{
    /// <summary>
    /// Standard API response wrapper for all endpoints.
    /// Provides consistent structure for success and error responses across the API.
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Message summarizing the operation result.
        /// E.g., "User registered successfully" or "Invalid credentials"
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Indicates if the operation was successful.
        /// True if StatusCode is 2xx, False otherwise.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The response data payload.
        /// Null if operation failed or no data to return.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// List of errors if operation failed.
        /// Null if operation succeeded.
        /// </summary>
        public ErrorDetails? Errors { get; set; }

        /// <summary>
        /// Timestamp when the response was generated (UTC).
        /// Useful for tracking and debugging.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional correlation ID for tracing requests across services.
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Creates a successful API response with data.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                IsSuccess = true,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failure API response with errors.
        /// </summary>
        public static ApiResponse<T> FailureResponse(string message, ErrorDetails? errors = null, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                IsSuccess = false,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Error details returned in API response.
    /// Contains specific field errors and general error information.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Error code for programmatic handling.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// General error message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Dictionary of field-specific errors.
        /// Key is field name, value is list of error messages for that field.
        /// </summary>
        public Dictionary<string, List<string>>? FieldErrors { get; set; }

        /// <summary>
        /// Stack trace for debugging (only in development).
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception details if applicable.
        /// </summary>
        public ErrorDetails? InnerError { get; set; }
    }
}
