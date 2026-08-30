using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Commands;

public sealed record DisableUserCommand(
    Guid AdminId,
    Guid TargetUserId
) : IRequest<Unit>;

public sealed class DisableUserCommandHandler
    : IRequestHandler<DisableUserCommand, Unit> {
    private readonly IUserService _userService;
    private readonly IUserAdminService _userAdminService;
    private readonly ITokenService _tokenService;

    public DisableUserCommandHandler(
        IUserService userService,
        IUserAdminService userAdminService,
        ITokenService tokenService) {
        _userService = userService;
        _userAdminService = userAdminService;
        _tokenService = tokenService;
    }

    public async Task<Unit> Handle(
        DisableUserCommand request,
        CancellationToken cancellationToken) {
        Guard.AgainstBusinessRule(
            request.AdminId == request.TargetUserId,
            "You cannot disable your own account.");

        var user = await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);

        Guard.AgainstNotFound(user, "User", request.TargetUserId);

        await _userAdminService.DisableUserAsync(
            request.TargetUserId, cancellationToken);

        await _tokenService.RevokeAllUserTokensAsync(
            request.TargetUserId, cancellationToken);

        return Unit.Value;
    }
}
