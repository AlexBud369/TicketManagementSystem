using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface ITokenService {
    TokenResponse GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);

    string GenerateRefreshToken();

    Task SaveRefreshTokenAsync(
        Guid userId,
        string refreshToken,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    Task<Guid> ValidateRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(
        string refreshToken,
        string replacedByToken,
        CancellationToken cancellationToken = default);

    Task RevokeAllUserTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
