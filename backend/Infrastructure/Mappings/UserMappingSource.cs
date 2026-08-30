using Application.DTOs.Users;
using Domain.Entities;
using Infrastructure.Persistence.Identity;

namespace Infrastructure.Mappings;

public sealed class UserMappingSource {
    public required User Profile { get; init; }
    public required ApplicationUser Identity { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
}
