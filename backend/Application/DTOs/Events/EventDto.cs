namespace Application.DTOs.Events;

public sealed class EventDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;

    public Guid OrganizerId { get; init; }
    public string OrganizerName { get; init; } = string.Empty;

    public string Location { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }

    public string? BannerImageUrl { get; init; }

    public int MaxCapacity { get; init; }
    public int AvailableTickets { get; init; }
    public int SoldTickets { get; init; }
    public decimal TicketPrice { get; init; }

    public bool IsPublished { get; init; }
    public bool IsSoldOut { get; init; }
    public bool AllowCancellation { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
