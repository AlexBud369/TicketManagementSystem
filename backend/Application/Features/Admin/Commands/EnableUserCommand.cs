using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Commands;

public sealed record EnableUserCommand(
    Guid TargetUserId
) : IRequest<Unit>;

public sealed class EnableUserCommandHandler
    : IRequestHandler<EnableUserCommand, Unit> {
    private readonly IUserService _userService;
    private readonly IUserAdminService _userAdminService;

    public EnableUserCommandHandler(
        IUserService userService,
        IUserAdminService userAdminService) {
        _userService = userService;
        _userAdminService = userAdminService;
    }

    public async Task<Unit> Handle(
        EnableUserCommand request,
        CancellationToken cancellationToken) {
        var user = await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);

        Guard.AgainstNotFound(user, "User", request.TargetUserId);

        await _userAdminService.EnableUserAsync(
            request.TargetUserId, cancellationToken);

        return Unit.Value;
    }
}
