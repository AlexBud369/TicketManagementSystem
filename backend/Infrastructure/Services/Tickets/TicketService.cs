using Application.DTOs.Tickets;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Tickets;

public sealed class TicketService : ITicketService {
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public TicketService(AppDbContext dbContext, IMapper mapper) {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<TicketDto>> GetMyTicketsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var tickets = await QueryTickets()
            .Where(ticket => ticket.UserId == userId)
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync(cancellationToken);

        return tickets.Select(ticket => _mapper.Map<TicketDto>(ticket)).ToList();
    }

    public async Task<TicketDto> GetByIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default) {
        var ticket = await QueryTickets()
            .FirstOrDefaultAsync(item => item.Id == ticketId, cancellationToken);

        if (ticket is null) {
            throw AppException.NotFound("Ticket", ticketId);
        }

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<byte[]> GeneratePdfAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default) {
        var ticket = await GetByIdAsync(ticketId, cancellationToken);
        return TicketPdfGenerator.Generate(ticket);
    }

    public async Task<TicketValidationResultDto> ValidateAsync(
        string qrCode,
        CancellationToken cancellationToken = default) {
        var payload = qrCode.Trim();

        var ticket = await _dbContext.Tickets
            .Include(item => item.User)
            .Include(item => item.Order)
                .ThenInclude(order => order!.OrderItems)
            .Include(item => item.EventEntity)
            .ThenInclude(eventEntity => eventEntity!.BannerImage)
            .FirstOrDefaultAsync(item => item.QrCode == payload, cancellationToken);

        if (ticket is null) {
            return new TicketValidationResultDto {
                Status = TicketScanStatus.Invalid
            };
        }

        if (ticket.Status == TicketStatus.Used) {
            return new TicketValidationResultDto {
                Status = TicketScanStatus.AlreadyUsed,
                Ticket = _mapper.Map<TicketDto>(ticket)
            };
        }

        if (ticket.Status != TicketStatus.Active) {
            return new TicketValidationResultDto {
                Status = TicketScanStatus.Invalid,
                Ticket = _mapper.Map<TicketDto>(ticket)
            };
        }

        ticket.Status = TicketStatus.Used;
        ticket.UsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TicketValidationResultDto {
            Status = TicketScanStatus.Valid,
            Ticket = _mapper.Map<TicketDto>(ticket)
        };
    }

    private IQueryable<Ticket> QueryTickets() {
        return _dbContext.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.User)
            .Include(ticket => ticket.Order)
                .ThenInclude(order => order!.OrderItems)
            .Include(ticket => ticket.EventEntity)
            .ThenInclude(eventEntity => eventEntity!.BannerImage);
    }
}
