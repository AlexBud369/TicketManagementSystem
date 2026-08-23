namespace Application.Exceptions;

public enum AppErrorCode {
    NotFound,
    Unauthorized,
    Forbidden,
    BusinessRuleViolation,
    ValidationFailed,
    Conflict
}

public sealed class AppException : Exception {
    public AppErrorCode Code { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    private AppException(AppErrorCode code, string message)
        : base(message) {
        Code = code;
        Errors = null;
    }

    private AppException(AppErrorCode code, string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message) {
        Code = code;
        Errors = errors;
    }

    public static AppException NotFound(string entityName, object key) =>
        new(AppErrorCode.NotFound, $"{entityName} with id '{key}' was not found.");

    public static AppException NotFound(string message) =>
        new(AppErrorCode.NotFound, message);

    public static AppException Unauthorized(string message = "You are not authorized to perform this action.") =>
        new(AppErrorCode.Unauthorized, message);

    public static AppException Forbidden(string message = "You do not have permission to perform this action.") =>
        new(AppErrorCode.Forbidden, message);

    public static AppException BusinessRule(string message) =>
        new(AppErrorCode.BusinessRuleViolation, message);

    public static AppException Conflict(string message) =>
        new(AppErrorCode.Conflict, message);

    public static AppException Validation(string field, string error) =>
        new(
            AppErrorCode.ValidationFailed,
            "One or more validation errors occurred.",
            new Dictionary<string, string[]> { { field, [error] } });

    public static AppException Validation(IDictionary<string, string[]> errors) =>
        new(
            AppErrorCode.ValidationFailed,
            "One or more validation errors occurred.",
            new Dictionary<string, string[]>(errors));
}
