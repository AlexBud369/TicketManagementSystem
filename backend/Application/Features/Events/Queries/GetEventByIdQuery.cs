using Application.DTOs.Events;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Events.Queries;

public sealed record GetEventByIdQuery(Guid EventId) : IRequest<EventDto>;

public sealed class GetEventByIdQueryHandler
    : IRequestHandler<GetEventByIdQuery, EventDto> {
    private readonly IEventService _eventService;

    public GetEventByIdQueryHandler(IEventService eventService) {
        _eventService = eventService;
    }

    public async Task<EventDto> Handle(
        GetEventByIdQuery request,
        CancellationToken cancellationToken) {
        return await _eventService.GetByIdAsync(
            request.EventId,
            cancellationToken);
    }
}
