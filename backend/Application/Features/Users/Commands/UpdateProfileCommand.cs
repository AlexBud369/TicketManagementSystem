using Application.DTOs.Users;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Phone,
    string Address
) : IRequest<UserDto>;

public sealed class UpdateProfileCommandHandler
    : IRequestHandler<UpdateProfileCommand, UserDto> {
    private readonly IUserService _userService;

    public UpdateProfileCommandHandler(IUserService userService) {
        _userService = userService;
    }

    public async Task<UserDto> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken) {
        await _userService.UpdateProfileAsync(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Address,
            cancellationToken);

        return await _userService.GetByIdAsync(
            request.UserId, cancellationToken);
    }
}
