namespace Application.DTOs.Dashboard;

public sealed class RevenueByEventDto {
    public Guid EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int TicketsSold { get; init; }
}
