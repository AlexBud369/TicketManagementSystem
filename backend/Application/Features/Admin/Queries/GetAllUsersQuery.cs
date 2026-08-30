using Application.Common.Models;
using Application.DTOs.Users;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Queries;

public sealed record GetAllUsersQuery(
    string Search,
    string RoleFilter,
    string StatusFilter,
    int Page,
    int PageSize
) : IRequest<PagedResult<UserListDto>>;

public sealed class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserListDto>> {
    private readonly IUserAdminService _userAdminService;

    public GetAllUsersQueryHandler(IUserAdminService userAdminService) {
        _userAdminService = userAdminService;
    }

    public async Task<PagedResult<UserListDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken) {
        var pagination = new PaginationParams {
            Page = request.Page,
            PageSize = request.PageSize
        };

        return await _userAdminService.GetAllUsersAsync(
            request.Search,
            request.RoleFilter,
            request.StatusFilter,
            pagination.Page,
            pagination.PageSize,
            cancellationToken);
    }
}
