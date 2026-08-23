using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Commands;

public sealed record UpdateUserByAdminCommand(
    Guid TargetUserId,
    string FirstName,
    string LastName,
    string Phone,
    string Address
) : IRequest<UserDto>;

public sealed class UpdateUserByAdminCommandHandler
    : IRequestHandler<UpdateUserByAdminCommand, UserDto> {
    private readonly IUserService _userService;
    private readonly IUserAdminService _userAdminService;

    public UpdateUserByAdminCommandHandler(
        IUserService userService,
        IUserAdminService userAdminService) {
        _userService = userService;
        _userAdminService = userAdminService;
    }

    public async Task<UserDto> Handle(
        UpdateUserByAdminCommand request,
        CancellationToken cancellationToken) {
        var user = await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);

        Guard.AgainstNotFound(user, "User", request.TargetUserId);

        await _userAdminService.UpdateUserAsync(
            request.TargetUserId,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Address,
            cancellationToken);

        return await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);
    }
}
