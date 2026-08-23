using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Orders.Commands;

public sealed record CancelOrderCommand(
    Guid OrderId,
    Guid UserId,
    bool IsAdmin
) : IRequest<Unit>;

public sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, Unit> {
    private readonly IOrderService _orderService;
    private readonly IEventService _eventService;

    public CancelOrderCommandHandler(
        IOrderService orderService,
        IEventService eventService) {
        _orderService = orderService;
        _eventService = eventService;
    }

    public async Task<Unit> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken) {
        var order = await _orderService.GetByIdAsync(
            request.OrderId, cancellationToken);

        Guard.AgainstNotFound(order, "Order", request.OrderId);

        Guard.AgainstUnauthorizedAccess(
            request.UserId, order.UserId, request.IsAdmin);

        Guard.AgainstBusinessRule(
            order.Status is OrderStatus.Cancelled or OrderStatus.Refunded,
            "This order cannot be cancelled.");

        if (!request.IsAdmin) {
            var evt = await _eventService.GetByIdAsync(
                order.EventId, cancellationToken);

            Guard.AgainstBusinessRule(
                !evt.AllowCancellation,
                "This event does not allow cancellations.");

            Guard.AgainstBusinessRule(
                !order.CanBeCancelled,
                "This order can no longer be cancelled.");
        }

        await _orderService.CancelAsync(
            request.OrderId,
            request.UserId,
            request.IsAdmin,
            cancellationToken);

        return Unit.Value;
    }
}
