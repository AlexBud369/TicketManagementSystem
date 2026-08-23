using Application.DTOs.Auth;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthResponse> {
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IUserService userService,
        ITokenService tokenService) {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken) {
        var userId = await _tokenService.ValidateRefreshTokenAsync(
            request.RefreshToken, cancellationToken);

        var user = await _userService.GetByIdAsync(userId, cancellationToken);

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, user.Roles);

        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.RevokeRefreshTokenAsync(
            request.RefreshToken, newRefreshToken, cancellationToken);

        await _tokenService.SaveRefreshTokenAsync(
            user.Id, newRefreshToken, false, cancellationToken);

        return new AuthResponse {
            AccessToken = newAccessToken.AccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = newAccessToken.AccessTokenExpiresAt,
            User = user
        };
    }
}
