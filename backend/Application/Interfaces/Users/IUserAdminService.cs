using Application.Common.Models;
using Application.DTOs.Users;

namespace Application.Interfaces;

public interface IUserAdminService {
    Task<PagedResult<UserListDto>> GetAllUsersAsync(
        string search,
        string roleFilter,
        string statusFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task DisableUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task EnableUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ChangeUserRoleAsync(
        Guid userId,
        string newRole,
        CancellationToken cancellationToken = default);

    Task UpdateUserAsync(
        Guid userId,
        string firstName,
        string lastName,
        string phone,
        string address,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
