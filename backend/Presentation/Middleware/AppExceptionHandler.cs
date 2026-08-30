using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Presentation.Middleware;

public sealed class AppExceptionHandler : IExceptionHandler {
    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger) {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken) {
        if (exception is not AppException appException) {
            return false;
        }

        var statusCode = appException.Code switch {
            AppErrorCode.NotFound => StatusCodes.Status404NotFound,
            AppErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
            AppErrorCode.Forbidden => StatusCodes.Status403Forbidden,
            AppErrorCode.Conflict => StatusCodes.Status409Conflict,
            AppErrorCode.ValidationFailed => StatusCodes.Status400BadRequest,
            AppErrorCode.BusinessRuleViolation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        _logger.LogWarning(
            exception,
            "Application exception {Code}: {Message}",
            appException.Code,
            appException.Message);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new {
            code = appException.Code.ToString(),
            message = appException.Message,
            errors = appException.Errors
        }, cancellationToken);

        return true;
    }
}
