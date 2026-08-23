using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Commands;

public sealed record ChangeUserRoleCommand(
    Guid AdminId,
    Guid TargetUserId,
    string NewRole
) : IRequest<Unit>;

public sealed class ChangeUserRoleCommandHandler
    : IRequestHandler<ChangeUserRoleCommand, Unit> {
    private readonly IUserService _userService;
    private readonly IUserAdminService _userAdminService;
    private readonly ITokenService _tokenService;

    public ChangeUserRoleCommandHandler(
        IUserService userService,
        IUserAdminService userAdminService,
        ITokenService tokenService) {
        _userService = userService;
        _userAdminService = userAdminService;
        _tokenService = tokenService;
    }

    public async Task<Unit> Handle(
        ChangeUserRoleCommand request,
        CancellationToken cancellationToken) {
        Guard.AgainstBusinessRule(
            request.AdminId == request.TargetUserId,
            "You cannot change your own role.");

        var user = await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);

        Guard.AgainstNotFound(user, "User", request.TargetUserId);

        await _userAdminService.ChangeUserRoleAsync(
            request.TargetUserId, request.NewRole, cancellationToken);

        await _tokenService.RevokeAllUserTokensAsync(
            request.TargetUserId, cancellationToken);

        return Unit.Value;
    }
}
