using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<Unit>;

public sealed class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, Unit> {
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public ChangePasswordCommandHandler(
        IPasswordService passwordService,
        ITokenService tokenService) {
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Unit> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken) {
        await _passwordService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        await _tokenService.RevokeAllUserTokensAsync(
            request.UserId, cancellationToken);

        return Unit.Value;
    }
}
