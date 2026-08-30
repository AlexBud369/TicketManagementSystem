using Application.Exceptions;

namespace Application.Common;

public static class Guard {
    public static T AgainstNotFound<T>(T? entity, string entityName, object key) {
        if (entity is null) {
            throw AppException.NotFound(entityName, key);
        }

        return entity;
    }


    public static string AgainstNullOrEmpty(string? value, string paramName) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
        } 

        return value;
    }

    public static T AgainstNull<T>(T? value, string paramName) {
        if (value is null) {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
        }

        return value;
    }

    public static void AgainstBusinessRule(bool condition, string message) {
        if (condition) {
            throw AppException.BusinessRule(message);
        }
            
    }

    public static void AgainstUnauthorizedAccess(
        Guid currentUserId,
        Guid resourceOwnerId,
        bool isAdmin = false) {
        if (isAdmin) {
            return;
        }

        if (currentUserId != resourceOwnerId) {
            throw AppException.Unauthorized("You do not have access to this resource.");
        }
           
    }

    public static void AgainstDuplicate(bool condition, string message) {
        if (condition) {
            throw AppException.Conflict(message);
        }
           
    }
}
