using System.Security.Claims;
using Presentation.Authorization;

namespace Presentation.Extensions;

public static class ClaimsPrincipalExtensions {
    public static Guid GetUserId(this ClaimsPrincipal user) {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId)) {
            throw new UnauthorizedAccessException("The access token has no user id.");
        }

        return userId;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) {
        return user.IsInRole(AppRoleNames.Admin);
    }
}
