using Application.Common.Models;
using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Orders.Models;
using MediatR;

namespace Application.Features.Orders.Queries;

public sealed record GetAllOrdersQuery(
    string Search,
    string StatusFilter,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page,
    int PageSize
) : IRequest<PagedResult<OrderListDto>>;

public sealed class GetAllOrdersQueryHandler
    : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderListDto>> {
    private readonly IOrderService _orderService;

    public GetAllOrdersQueryHandler(IOrderService orderService) {
        _orderService = orderService;
    }

    public async Task<PagedResult<OrderListDto>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken) {
        var pagination = new PaginationParams {
            Page = request.Page,
            PageSize = request.PageSize
        };

        var filter = new OrderFilterRequest {
            Search = request.Search,
            StatusFilter = request.StatusFilter,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };

        return await _orderService.GetAllAsync(
            filter, cancellationToken);
    }
}
