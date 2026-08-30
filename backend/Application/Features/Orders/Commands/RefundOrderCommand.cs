using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Orders.Commands;

public sealed record RefundOrderCommand(
    Guid OrderId
) : IRequest<Unit>;

public sealed class RefundOrderCommandHandler
    : IRequestHandler<RefundOrderCommand, Unit> {
    private readonly IOrderService _orderService;

    public RefundOrderCommandHandler(IOrderService orderService) {
        _orderService = orderService;
    }

    public async Task<Unit> Handle(
        RefundOrderCommand request,
        CancellationToken cancellationToken) {
        var order = await _orderService.GetByIdAsync(
            request.OrderId, cancellationToken);

        Guard.AgainstBusinessRule(
            order.Status != OrderStatus.Paid,
            "Only paid orders can be refunded.");

        await _orderService.RefundAsync(
            request.OrderId, cancellationToken);

        return Unit.Value;
    }
}
