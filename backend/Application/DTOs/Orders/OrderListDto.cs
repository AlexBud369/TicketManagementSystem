using Domain.Enums;

namespace Application.DTOs.Orders;

public sealed class OrderListDto {
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string EventTitle { get; init; } = string.Empty;
    public DateTime EventDate { get; init; }
    public string? BuyerName { get; init; }
    public int Quantity { get; init; }
    public decimal Total { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
