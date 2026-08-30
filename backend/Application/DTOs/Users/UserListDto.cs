using Domain.Enums;

namespace Application.DTOs.Users;

public sealed class UserListDto {
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public IEnumerable<string> Roles { get; init; } = Enumerable.Empty<string>();
    public UserStatus Status { get; init; }
    public int OrdersCount { get; init; }
}
