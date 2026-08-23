using Application.DTOs.Users;

namespace Application.Interfaces;

public interface IUserService {
    Task<UserDto> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserDto> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(
        Guid userId,
        string firstName,
        string lastName,
        string phone,
        string address,
        CancellationToken cancellationToken = default);

    Task UpdateAvatarAsync(
        Guid userId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}
