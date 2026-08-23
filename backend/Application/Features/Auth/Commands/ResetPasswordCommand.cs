using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<Unit>;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, Unit> {
    private readonly IPasswordService _passwordService;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public ResetPasswordCommandHandler(
        IPasswordService passwordService,
        IUserService userService,
        ITokenService tokenService) {
        _passwordService = passwordService;
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<Unit> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken) {
        await _passwordService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            cancellationToken);

        var user = await _userService.GetByEmailAsync(
            request.Email, cancellationToken);

        await _tokenService.RevokeAllUserTokensAsync(
            user.Id, cancellationToken);

        return Unit.Value;
    }
}
