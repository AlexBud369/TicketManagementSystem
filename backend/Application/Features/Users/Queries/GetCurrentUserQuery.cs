using Application.DTOs.Users;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Users.Queries;

public sealed record GetCurrentUserQuery(
    Guid UserId
) : IRequest<UserDto>;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, UserDto> {
    private readonly IUserService _userService;

    public GetCurrentUserQueryHandler(IUserService userService) {
        _userService = userService;
    }

    public async Task<UserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken) {
        return await _userService.GetByIdAsync(
            request.UserId, cancellationToken);
    }
}
