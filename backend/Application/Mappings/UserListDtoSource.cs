using Domain.Entities;

namespace Application.Mappings;

public sealed class UserListDtoSource {
    public required User User { get; init; }
    public IEnumerable<string> Roles { get; init; } = [];
    public int OrdersCount { get; init; }
}
