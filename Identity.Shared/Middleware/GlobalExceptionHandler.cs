using System.Text.Json;
using Identity.Shared.Response;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Shared.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var result = exception switch
        {
            FluentValidation.ValidationException fluentValidationEx => HandleFluentValidationException(fluentValidationEx),
            BusinessLogicException businessEx => HandleBusinessLogicException(businessEx),

            UnauthorizedAccessException => HandleUnauthorizedException(),
            KeyNotFoundException => HandleNotFoundException(),

            ArgumentNullException argNullEx => HandleArgumentNullException(argNullEx),
            ArgumentException argEx => HandleArgumentException(argEx),

            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),

            TaskCanceledException taskCanceledEx when taskCanceledEx.InnerException is TimeoutException => HandleTimeoutException(),
            TimeoutException => HandleTimeoutException(),
            OperationCanceledException => HandleOperationCancelledException(),

            _ => HandleGenericException(exception)
        };

        LogException(exception, httpContext);

        httpContext.Response.StatusCode = result.StatusCode;
        httpContext.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

        return true;
    }

    private static Result HandleFluentValidationException(FluentValidation.ValidationException ex)
    {
        var errors = ex.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}").ToList();
        return Result.Failure(errors, StatusCodes.Status400BadRequest);
    }

    private static Result HandleBusinessLogicException(BusinessLogicException ex)
    {
        return Result.Failure(ex.Message, StatusCodes.Status400BadRequest);
    }

    private static Result HandleUnauthorizedException()
    {
        return Result.Failure("Authentication is required to access this resource.", StatusCodes.Status401Unauthorized);
    }

    private static Result HandleNotFoundException()
    {
        return Result.Failure("The requested resource was not found.", StatusCodes.Status404NotFound);
    }

    private Result HandleArgumentException(ArgumentException ex)
    {
        return Result.Failure(
            _environment.IsDevelopment() ? ex.Message : "Invalid argument provided.",
            StatusCodes.Status400BadRequest
        );
    }

    private Result HandleArgumentNullException(ArgumentNullException ex)
    {
        return Result.Failure(
            _environment.IsDevelopment() ? ex.Message : "A required parameter was not provided.",
            StatusCodes.Status400BadRequest
        );
    }

    private Result HandleInvalidOperationException(InvalidOperationException ex)
    {
        return Result.Failure(
            _environment.IsDevelopment() ? ex.Message : "The requested operation is not valid in the current state.",
            StatusCodes.Status400BadRequest
        );
    }

    private static Result HandleTimeoutException()
    {
        return Result.Failure("The request timed out. Please try again later.", StatusCodes.Status408RequestTimeout);
    }

    private static Result HandleOperationCancelledException()
    {
        return Result.Failure("The operation was cancelled.", StatusCodes.Status400BadRequest);
    }

    private Result HandleGenericException(Exception ex)
    {
        return Result.Failure(
            _environment.IsDevelopment() ? $"An error occurred: {ex.Message}" : "An unexpected error occurred. Please try again later.",
            StatusCodes.Status500InternalServerError
        );
    }

    private void LogException(Exception exception, HttpContext context)
    {
        var logLevel = exception switch
        {
            FluentValidation.ValidationException => LogLevel.Warning,
            BusinessLogicException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            KeyNotFoundException => LogLevel.Information,
            ArgumentException or ArgumentNullException => LogLevel.Warning,
            OperationCanceledException => LogLevel.Information,
            _ => LogLevel.Error
        };

        var logMessage = "Exception occurred during request processing. " +
                       "TraceId: {TraceId}, Path: {Path}, Method: {Method}, StatusCode: {StatusCode}, Exception: {ExceptionType}";

        _logger.Log(logLevel, exception, logMessage,
            context.TraceIdentifier,
            context.Request.Path,
            context.Request.Method,
            context.Response.StatusCode,
            exception.GetType().Name);
    }
}
