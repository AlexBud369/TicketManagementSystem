using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe
) : IRequest<AuthResponse>;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponse> {
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IAuthService authService,
        ITokenService tokenService) {
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken) {
        var user = await _authService.ValidateCredentialsAsync(
            request.Email, request.Password, cancellationToken);

        Guard.AgainstBusinessRule(
            user.Status == UserStatus.Disabled,
            "This account has been disabled.");

        Guard.AgainstBusinessRule(
            !user.EmailConfirmed,
            "Please confirm your email before signing in.");

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, user.Roles);

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshTokenAsync(
            user.Id, refreshToken, request.RememberMe, cancellationToken);

        return new AuthResponse {
            AccessToken = accessToken.AccessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessToken.AccessTokenExpiresAt,
            User = user
        };
    }
}
