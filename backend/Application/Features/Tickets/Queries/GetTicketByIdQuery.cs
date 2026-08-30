using Application.Common;
using Application.DTOs.Tickets;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Tickets.Queries;

public sealed record GetTicketByIdQuery(
    Guid TicketId,
    Guid UserId,
    bool IsAdmin
) : IRequest<TicketDto>;

public sealed class GetTicketByIdQueryHandler
    : IRequestHandler<GetTicketByIdQuery, TicketDto> {
    private readonly ITicketService _ticketService;

    public GetTicketByIdQueryHandler(ITicketService ticketService) {
        _ticketService = ticketService;
    }

    public async Task<TicketDto> Handle(
        GetTicketByIdQuery request,
        CancellationToken cancellationToken) {
        var ticket = await _ticketService.GetByIdAsync(
            request.TicketId, cancellationToken);

        Guard.AgainstUnauthorizedAccess(
            request.UserId, ticket.UserId, request.IsAdmin);

        return ticket;
    }
}
