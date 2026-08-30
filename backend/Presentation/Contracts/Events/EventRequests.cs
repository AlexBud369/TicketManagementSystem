namespace Presentation.Contracts.Events;

public sealed record CreateEventRequest(
    string Title,
    string Description,
    Guid CategoryId,
    string Location,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int MaximumCapacity,
    decimal TicketPrice,
    bool AllowCancellation,
    double? Latitude,
    double? Longitude);

public sealed record UpdateEventRequest(
    string Title,
    string Description,
    Guid CategoryId,
    string Location,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int MaximumCapacity,
    decimal TicketPrice,
    bool AllowCancellation,
    double? Latitude,
    double? Longitude);
