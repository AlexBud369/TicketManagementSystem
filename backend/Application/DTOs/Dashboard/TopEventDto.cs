namespace Application.DTOs.Dashboard;

public sealed class TopEventDto {
    public Guid EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public string? BannerImageUrl { get; init; }
    public DateTime EventDate { get; init; }
    public int TicketsSold { get; init; }
    public int MaxCapacity { get; init; }
    public decimal Revenue { get; init; }
    public double OccupancyRate { get; init; }
}
