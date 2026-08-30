using Domain.Enums;

namespace Application.DTOs.Users;

public sealed class UserDto {
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public Guid? AvatarImageId { get; init; }
    public string? AvatarUrl { get; init; }
    public UserStatus Status { get; init; }
    public IEnumerable<string> Roles { get; init; } = Enumerable.Empty<string>();
    public DateTime CreatedAt { get; init; }
}
