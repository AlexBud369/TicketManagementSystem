using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Tickets.Queries;

public sealed record DownloadTicketQuery(
    Guid TicketId,
    Guid UserId,
    bool IsAdmin
) : IRequest<byte[]>;

public sealed class DownloadTicketQueryHandler
    : IRequestHandler<DownloadTicketQuery, byte[]> {
    private readonly ITicketService _ticketService;

    public DownloadTicketQueryHandler(ITicketService ticketService) {
        _ticketService = ticketService;
    }

    public async Task<byte[]> Handle(
        DownloadTicketQuery request,
        CancellationToken cancellationToken) {
        var ticket = await _ticketService.GetByIdAsync(
            request.TicketId, cancellationToken);

        Guard.AgainstUnauthorizedAccess(
            request.UserId, ticket.UserId, request.IsAdmin);

        return await _ticketService.GeneratePdfAsync(
            request.TicketId, cancellationToken);
    }
}
