namespace Application.Interfaces.Events.Models;

public sealed class UpdateEventRequest {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaximumCapacity { get; set; }
    public decimal TicketPrice { get; set; }
    public bool AllowCancellation { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
