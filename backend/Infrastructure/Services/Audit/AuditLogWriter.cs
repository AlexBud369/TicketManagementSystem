using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Audit;

public sealed class AuditLogWriter {
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogWriter(
        AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor) {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Append(
        AuditAction action,
        string entityName,
        string? entityId,
        Guid? userId,
        string? oldValues = null,
        string? newValues = null) {
        var http = _httpContextAccessor.HttpContext;

        _dbContext.AuditLogs.Add(new Domain.Entities.AuditLog {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ResolveIp(http),
            UserAgent = Truncate(http?.Request.Headers.UserAgent.ToString(), 512)
        });
    }

    public Task SaveAsync(CancellationToken cancellationToken = default) {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? ResolveIp(HttpContext? http) {
        if (http is null) {
            return null;
        }

        var forwarded = http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded)) {
            return Truncate(forwarded.Split(',')[0].Trim(), 64);
        }

        return Truncate(http.Connection.RemoteIpAddress?.ToString(), 64);
    }

    private static string? Truncate(string? value, int maxLength) {
        if (string.IsNullOrWhiteSpace(value)) {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
