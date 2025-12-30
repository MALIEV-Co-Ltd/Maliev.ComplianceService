using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Maliev.ComplianceService.Api.Middlewares;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next request delegate.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles the exception and writes a consistent error response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var errorCode = "INTERNAL_SERVER_ERROR";
        var message = "An error occurred while processing your request";

        // Map specific exceptions to error codes and status codes
        switch (exception)
        {
            case System.ComponentModel.DataAnnotations.ValidationException:
                code = HttpStatusCode.BadRequest;
                errorCode = exception.Message; // Handler throws with error code as message
                message = exception.Message == "INVALID_DATE_RANGE" 
                    ? "The issue date cannot be after the expiration date"
                    : "Validation failed";
                break;

            case ArgumentNullException:
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                errorCode = "INVALID_ARGUMENT";
                message = exception.Message;
                break;

            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = exception.Message;
                break;

            case UnauthorizedAccessException:
                code = HttpStatusCode.Forbidden;
                errorCode = "NOT_AUTHORIZED";
                message = "You do not have permission to perform this action";
                break;

            case DbUpdateConcurrencyException:
                code = HttpStatusCode.Conflict;
                errorCode = "CONCURRENT_MODIFICATION";
                message = "The record was modified by another user. Please refresh and try again";
                break;

            case DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate key") == true:
                code = HttpStatusCode.Conflict;
                errorCode = "DUPLICATE_AUTHORIZATION";
                message = "An active authorization of this type already exists for this employee";
                break;

            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                errorCode = "INVALID_OPERATION";
                message = exception.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var result = JsonSerializer.Serialize(new
        {
            error = new
            {
                code = errorCode,
                message = message,
                details = context.Request.Path.Value
            }
        });

        return context.Response.WriteAsync(result);
    }
}

/// <summary>
/// Extension method for registering the middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Registers the exception handling middleware in the HTTP request pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
