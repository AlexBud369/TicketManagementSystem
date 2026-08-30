using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Events.Commands;

public sealed record DeleteEventCommand(Guid EventId) : IRequest<Unit>;

public sealed class DeleteEventCommandHandler
    : IRequestHandler<DeleteEventCommand, Unit> {
    private readonly IEventService _eventService;

    public DeleteEventCommandHandler(IEventService eventService) {
        _eventService = eventService;
    }

    public async Task<Unit> Handle(
        DeleteEventCommand request,
        CancellationToken cancellationToken) {
        var hasPaidOrders = await _eventService.HasPaidOrdersAsync(
            request.EventId, cancellationToken);

        Guard.AgainstBusinessRule(
            hasPaidOrders,
            "Cannot delete an event that has paid orders. Refund or cancel them first.");

        await _eventService.DeleteAsync(
            request.EventId,
            cancellationToken);

        return Unit.Value;
    }
}
