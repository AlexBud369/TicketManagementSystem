namespace Application.DTOs.Dashboard;

public sealed class DashboardStatsDto {
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int DisabledUsers { get; init; }

    public int TotalEvents { get; init; }
    public int PublishedEvents { get; init; }
    public int UpcomingEvents { get; init; }

    public int TicketsSold { get; init; }
    public decimal TotalRevenue { get; init; }

    public int TotalRefunds { get; init; }
    public decimal RefundedAmount { get; init; }

    public IEnumerable<MonthlySalesDto> MonthlySales { get; init; }
        = Enumerable.Empty<MonthlySalesDto>();

    public IEnumerable<RevenueByEventDto> RevenueByEvent { get; init; }
        = Enumerable.Empty<RevenueByEventDto>();

    public IEnumerable<TopEventDto> TopEvents { get; init; }
        = Enumerable.Empty<TopEventDto>();

    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}
