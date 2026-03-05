using BlindIdea.Application.Dtos.Common;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlindIdea.API.Middleware
{
    
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();
            
            switch (exception)
            {
                
                case ArgumentNullException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.FailureResponse(
                        $"Invalid input: {ex.ParamName} is required",
                        new ErrorDetails
                        {
                            Code = "INVALID_INPUT",
                            Message = ex.Message
                        },
                        (int)HttpStatusCode.BadRequest
                    );
                    break;

                case ArgumentException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.FailureResponse(
                        $"Invalid input: {ex.Message}",
                        new ErrorDetails
                        {
                            Code = "INVALID_ARGUMENT",
                            Message = ex.Message
                        },
                        (int)HttpStatusCode.BadRequest
                    );
                    break;

                case UnauthorizedAccessException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.FailureResponse(
                        "Unauthorized access",
                        new ErrorDetails
                        {
                            Code = "UNAUTHORIZED",
                            Message = ex.Message
                        },
                        (int)HttpStatusCode.Unauthorized
                    );
                    break;

                case KeyNotFoundException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse<object>.FailureResponse(
                        "Resource not found",
                        new ErrorDetails
                        {
                            Code = "NOT_FOUND",
                            Message = ex.Message
                        },
                        (int)HttpStatusCode.NotFound
                    );
                    break;

                case InvalidOperationException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.FailureResponse(
                        "Invalid operation",
                        new ErrorDetails
                        {
                            Code = "INVALID_OPERATION",
                            Message = ex.Message
                        },
                        (int)HttpStatusCode.BadRequest
                    );
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse<object>.FailureResponse(
                        "An unexpected error occurred",
                        new ErrorDetails
                        {
                            Code = "INTERNAL_SERVER_ERROR",
                            Message = "An unexpected error occurred. Please try again later."
                        },
                        (int)HttpStatusCode.InternalServerError
                    );

                    _logger.LogError(exception, "Unhandled exception: {ExceptionType} - {Message}\n{StackTrace}",
                        exception.GetType().Name, exception.Message, exception.StackTrace);
                    
                    if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
                    {
                        response.Errors!.StackTrace = exception.StackTrace;
                    }
                    break;
            }

            response.TraceId = context.TraceIdentifier;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(response, jsonOptions);
            return context.Response.WriteAsync(json);
        }
    }
}