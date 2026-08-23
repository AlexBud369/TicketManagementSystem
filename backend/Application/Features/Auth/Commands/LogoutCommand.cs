using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record LogoutCommand(
    string RefreshToken
) : IRequest<Unit>;

public sealed class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, Unit> {
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(ITokenService tokenService) {
        _tokenService = tokenService;
    }

    public async Task<Unit> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken) {
        await _tokenService.RevokeRefreshTokenAsync(
            request.RefreshToken, string.Empty, cancellationToken);

        return Unit.Value;
    }
}
