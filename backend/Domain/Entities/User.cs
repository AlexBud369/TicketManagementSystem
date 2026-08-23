using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity {
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? AvatarImageId { get; set; }
    public string? Address { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    public Image? AvatarImage { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
