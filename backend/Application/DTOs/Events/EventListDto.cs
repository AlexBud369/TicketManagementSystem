namespace Application.DTOs.Events;

public sealed class EventListDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public TimeSpan StartTime { get; init; }
    public string? BannerImageUrl { get; init; }
    public decimal TicketPrice { get; init; }
    public int AvailableTickets { get; init; }
    public bool IsSoldOut { get; init; }
    public bool IsPublished { get; init; }
}
