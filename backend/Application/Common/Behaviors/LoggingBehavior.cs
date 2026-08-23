using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

#pragma warning disable CA2016

namespace Application.Common.Behaviors;

public sealed partial class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> {
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private const int SlowRequestThresholdMs = 500;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        LogHandling(_logger, requestName);

        try {
            var response = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > SlowRequestThresholdMs) {
                LogSlowRequest(_logger, requestName, elapsedMs);
            }
            else {
                LogHandled(_logger, requestName, elapsedMs);
            }

            return response;
        }
        catch (Exception ex) {
            stopwatch.Stop();
            LogFailed(_logger, requestName, stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogHandling(
        ILogger logger,
        string requestName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow request: {RequestName} took {ElapsedMs}ms")]
    private static partial void LogSlowRequest(
        ILogger logger,
        string requestName,
        long elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs}ms")]
    private static partial void LogHandled(
        ILogger logger,
        string requestName,
        long elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request {RequestName} failed after {ElapsedMs}ms")]
    private static partial void LogFailed(
        ILogger logger,
        string requestName,
        long elapsedMs,
        Exception ex);
}

#pragma warning restore CA2016
