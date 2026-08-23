using Application.Common;
using Application.DTOs.Orders;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Orders.Queries;

public sealed record GetOrderByIdQuery(
    Guid OrderId,
    Guid UserId,
    bool IsAdmin
) : IRequest<OrderDto>;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto> {
    private readonly IOrderService _orderService;

    public GetOrderByIdQueryHandler(IOrderService orderService) {
        _orderService = orderService;
    }

    public async Task<OrderDto> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken) {
        var order = await _orderService.GetByIdAsync(
            request.OrderId, cancellationToken);

        Guard.AgainstUnauthorizedAccess(
            request.UserId, order.UserId, request.IsAdmin);

        return order;
    }
}
