using Application.DTOs.Tickets;

namespace Application.Interfaces;

public interface ITicketService {
    Task<IReadOnlyCollection<TicketDto>> GetMyTicketsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TicketDto> GetByIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<byte[]> GeneratePdfAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketValidationResultDto> ValidateAsync(
        string qrCode,
        CancellationToken cancellationToken = default);
}
