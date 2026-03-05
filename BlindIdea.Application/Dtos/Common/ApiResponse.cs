using System;

namespace BlindIdea.Application.Dtos.Common
{
    
    public class ApiResponse<T>
    {
        
        public int StatusCode { get; set; }

        public string Message { get; set; } = null!;

        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public ErrorDetails? Errors { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? TraceId { get; set; }

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

    public class ErrorDetails
    {
        
        public string? Code { get; set; }

        public string? Message { get; set; }

        public Dictionary<string, List<string>>? FieldErrors { get; set; }

        public string? StackTrace { get; set; }

        public ErrorDetails? InnerError { get; set; }
    }
}