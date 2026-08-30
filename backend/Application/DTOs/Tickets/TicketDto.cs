using Domain.Enums;

namespace Application.DTOs.Tickets;

public sealed class TicketDto {
    public Guid Id { get; init; }
    public string TicketNumber { get; init; } = string.Empty;

    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;

    public Guid EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public DateTime EventDate { get; init; }
    public TimeSpan EventStartTime { get; init; }
    public string EventLocation { get; init; } = string.Empty;
    public string? EventBannerUrl { get; init; }

    public Guid UserId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public string BuyerEmail { get; init; } = string.Empty;

    public string QrCode { get; init; } = string.Empty;
    public string? SeatInfo { get; init; }
    public decimal Price { get; init; }

    public TicketStatus Status { get; init; }
    public DateTime? UsedAt { get; init; }
    public bool IsValid { get; init; }

    public DateTime CreatedAt { get; init; }
}
