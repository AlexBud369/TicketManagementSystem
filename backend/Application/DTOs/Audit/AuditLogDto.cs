using Domain.Enums;

namespace Application.DTOs.Audit;

public sealed class AuditLogDto {
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
    public AuditAction Action { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}
