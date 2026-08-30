using Application.DTOs.Users;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Queries;

public sealed record GetUserByIdQuery(
    Guid UserId
) : IRequest<UserDto>;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto> {
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService) {
        _userService = userService;
    }

    public async Task<UserDto> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken) {
        return await _userService.GetByIdAsync(
            request.UserId, cancellationToken);
    }
}
