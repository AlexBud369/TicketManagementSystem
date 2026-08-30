namespace Application.DTOs.Dashboard;

public sealed class MonthlySalesDto {
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public int TicketsSold { get; init; }
    public decimal Revenue { get; init; }
}
