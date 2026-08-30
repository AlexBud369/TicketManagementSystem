using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Ticket : BaseEntity {
    public string TicketNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public string? SeatInfo { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public DateTime? UsedAt { get; set; }

    public Order? Order { get; set; }
    public EventEntity? EventEntity { get; set; }
    public User? User { get; set; }
}
