using Application.DTOs.Tickets;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Tickets.Queries;

public sealed record GetMyTicketsQuery(
    Guid UserId
) : IRequest<IReadOnlyCollection<TicketDto>>;

public sealed class GetMyTicketsQueryHandler
    : IRequestHandler<GetMyTicketsQuery, IReadOnlyCollection<TicketDto>> {
    private readonly ITicketService _ticketService;

    public GetMyTicketsQueryHandler(ITicketService ticketService) {
        _ticketService = ticketService;
    }

    public async Task<IReadOnlyCollection<TicketDto>> Handle(
        GetMyTicketsQuery request,
        CancellationToken cancellationToken) {
        return await _ticketService.GetMyTicketsAsync(
            request.UserId, cancellationToken);
    }
}
