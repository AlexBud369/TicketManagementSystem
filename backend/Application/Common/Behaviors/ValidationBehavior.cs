using Application.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

#pragma warning disable CA2016

namespace Application.Common.Behaviors;

public sealed partial class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> {

    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger) {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken) {
        if (!_validators.Any()) {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        LogValidating(_logger, requestName);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .GroupBy(
                failure => failure.PropertyName,
                failure => failure.ErrorMessage)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray());

        if (failures.Count > 0) {
            LogValidationFailed(_logger, requestName, failures.Count);
            throw AppException.Validation(failures);
        }

        return await next();
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Validating request {RequestName}")]
    private static partial void LogValidating(
        ILogger logger,
        string requestName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Validation failed for {RequestName} with {ErrorCount} error(s)")]
    private static partial void LogValidationFailed(
        ILogger logger,
        string requestName,
        int errorCount);
}

#pragma warning restore CA2016
