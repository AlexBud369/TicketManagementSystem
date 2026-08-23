using Application.Common.Models;
using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Orders.Models;
using MediatR;

namespace Application.Features.Orders.Queries;

public sealed record GetMyOrdersQuery(
    Guid UserId,
    string StatusFilter,
    int Page,
    int PageSize
) : IRequest<PagedResult<OrderListDto>>;

public sealed class GetMyOrdersQueryHandler
    : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderListDto>> {
    private readonly IOrderService _orderService;

    public GetMyOrdersQueryHandler(IOrderService orderService) {
        _orderService = orderService;
    }

    public async Task<PagedResult<OrderListDto>> Handle(
        GetMyOrdersQuery request,
        CancellationToken cancellationToken) {
        var pagination = new PaginationParams {
            Page = request.Page,
            PageSize = request.PageSize
        };

        var filter = new OrderFilterRequest {
            StatusFilter = request.StatusFilter,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };

        return await _orderService.GetMyOrdersAsync(
            request.UserId, filter, cancellationToken);
    }
}
