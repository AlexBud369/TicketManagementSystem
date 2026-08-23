namespace Application.Interfaces.Events.Models;

public sealed class EventFilterRequest {
    public string Search { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? HasAvailableTickets { get; set; }
    public string SortBy { get; set; } = "date";
    public bool Descending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
