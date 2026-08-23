namespace Application.Interfaces.Orders.Models;

public sealed class OrderFilterRequest {
    public string Search { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
