using System;

namespace BlindIdea.Application.Dtos.Error
{
    
    public class ErrorResponse
    {
        
        public int StatusCode { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? ErrorCode { get; set; }

        public Dictionary<string, List<string>>? Errors { get; set; }

        public string? StackTrace { get; set; }

        public ErrorResponse? InnerError { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Path { get; set; }

        public string? TraceId { get; set; }

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

    public class ValidationErrorResponse
    {
        
        public Dictionary<string, List<string>> FieldErrors { get; set; } = new();

        public string Message { get; set; } = "One or more validation errors occurred";
    }

    public class PasswordValidationErrorResponse
    {
        
        public List<string> FailedRequirements { get; set; } = new();

        public string Message { get; set; } = "Password does not meet strength requirements";
    }
}