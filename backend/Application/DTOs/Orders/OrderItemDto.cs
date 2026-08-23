namespace Application.DTOs.Orders;

public sealed class OrderItemDto {
    public Guid Id { get; init; }
    public Guid? TicketId { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string? SeatInfo { get; init; }
    public decimal UnitPrice { get; init; }
}
