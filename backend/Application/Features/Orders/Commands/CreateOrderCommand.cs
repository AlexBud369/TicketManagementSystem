using Application.Common;
using Application.DTOs.Orders;
using Application.Interfaces;
using Application.Interfaces.Orders.Models;
using Domain.Scheduling;
using MediatR;

namespace Application.Features.Orders.Commands;

public sealed record CreateOrderCommand(
    Guid EventId,
    Guid UserId,
    int Quantity
) : IRequest<OrderDto>;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, OrderDto> {
    private readonly IOrderService _orderService;
    private readonly IEventService _eventService;

    public CreateOrderCommandHandler(
        IOrderService orderService,
        IEventService eventService) {
        _orderService = orderService;
        _eventService = eventService;
    }

    public async Task<OrderDto> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken) {
        var evt = await _eventService.GetByIdAsync(
            request.EventId, cancellationToken);

        Guard.AgainstNotFound(evt, "Event", request.EventId);

        Guard.AgainstBusinessRule(
            !evt.IsPublished,
            "Cannot purchase tickets for an unpublished event.");

        Guard.AgainstBusinessRule(
            EventSchedule.HasStarted(evt.Date, evt.StartTime, DateTime.UtcNow),
            "Cannot purchase tickets for a past event.");

        Guard.AgainstBusinessRule(
            evt.AvailableTickets < request.Quantity,
            $"Only {evt.AvailableTickets} tickets available.");

        var createRequest = new CreateOrderRequest {
            EventId = request.EventId,
            UserId = request.UserId,
            Quantity = request.Quantity
        };

        return await _orderService.CreateAsync(
            createRequest, cancellationToken);
    }
}
