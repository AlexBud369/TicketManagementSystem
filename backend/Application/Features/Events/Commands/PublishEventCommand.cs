using Application.Interfaces;
using MediatR;

namespace Application.Features.Events.Commands;

public sealed record PublishEventCommand(Guid EventId) : IRequest<Unit>;

public sealed class PublishEventCommandHandler
    : IRequestHandler<PublishEventCommand, Unit> {
    private readonly IEventService _eventService;

    public PublishEventCommandHandler(IEventService eventService) {
        _eventService = eventService;
    }

    public async Task<Unit> Handle(
        PublishEventCommand request,
        CancellationToken cancellationToken) {
        await _eventService.PublishAsync(
            request.EventId,
            cancellationToken);

        return Unit.Value;
    }
}
