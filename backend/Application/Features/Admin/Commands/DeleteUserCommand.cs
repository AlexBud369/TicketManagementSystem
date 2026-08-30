using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Commands;

public sealed record DeleteUserCommand(
    Guid AdminId,
    Guid TargetUserId
) : IRequest<Unit>;

public sealed class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, Unit> {
    private readonly IUserService _userService;
    private readonly IUserAdminService _userAdminService;
    private readonly ITokenService _tokenService;
    private readonly IStorageService _storageService;

    public DeleteUserCommandHandler(
        IUserService userService,
        IUserAdminService userAdminService,
        ITokenService tokenService,
        IStorageService storageService) {
        _userService = userService;
        _userAdminService = userAdminService;
        _tokenService = tokenService;
        _storageService = storageService;
    }

    public async Task<Unit> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken) {
        Guard.AgainstBusinessRule(
            request.AdminId == request.TargetUserId,
            "You cannot delete your own account.");

        var user = await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);

        Guard.AgainstNotFound(user, "User", request.TargetUserId);

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl)) {
            await _storageService.DeleteAsync(
                user.AvatarUrl, cancellationToken);
        }

        await _tokenService.RevokeAllUserTokensAsync(
            request.TargetUserId, cancellationToken);

        await _userAdminService.DeleteUserAsync(
            request.TargetUserId, cancellationToken);

        return Unit.Value;
    }
}
