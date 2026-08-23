using Domain.Enums;

namespace Application.DTOs.Tickets;

public sealed class TicketValidationResultDto {
    public TicketScanStatus Status { get; init; }
    public TicketDto? Ticket { get; init; }
}
