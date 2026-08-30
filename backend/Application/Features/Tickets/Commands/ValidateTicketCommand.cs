using Application.DTOs.Tickets;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Tickets.Commands;

public sealed record ValidateTicketCommand(
    string QrCode
) : IRequest<TicketValidationResultDto>;

public sealed class ValidateTicketCommandHandler
    : IRequestHandler<ValidateTicketCommand, TicketValidationResultDto> {
    private readonly ITicketService _ticketService;

    public ValidateTicketCommandHandler(ITicketService ticketService) {
        _ticketService = ticketService;
    }

    public async Task<TicketValidationResultDto> Handle(
        ValidateTicketCommand request,
        CancellationToken cancellationToken) {
        return await _ticketService.ValidateAsync(
            request.QrCode, cancellationToken);
    }
}
