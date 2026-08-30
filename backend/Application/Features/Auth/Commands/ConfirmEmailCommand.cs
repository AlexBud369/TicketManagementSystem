using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record ConfirmEmailCommand(
    string Email,
    string Token
) : IRequest<Unit>;

public sealed class ConfirmEmailCommandHandler
    : IRequestHandler<ConfirmEmailCommand, Unit> {
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;

    public ConfirmEmailCommandHandler(
        IUserService userService,
        IPasswordService passwordService) {
        _userService = userService;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken) {
        var user = await _userService.GetByEmailAsync(
            request.Email, cancellationToken);

        await _passwordService.ConfirmEmailAsync(
            user.Id, request.Token, cancellationToken);

        return Unit.Value;
    }
}
