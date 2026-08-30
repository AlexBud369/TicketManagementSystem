using Domain.Enums;

namespace Application.DTOs.Orders;

public sealed class OrderDto {
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;

    public Guid UserId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public string BuyerEmail { get; init; } = string.Empty;

    public Guid EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public DateTime EventDate { get; init; }
    public string EventLocation { get; init; } = string.Empty;
    public string? EventBannerUrl { get; init; }

    public int Quantity { get; init; }
    public decimal SubTotal { get; init; }
    public decimal ServiceFee { get; init; }
    public decimal Total { get; init; }

    public OrderStatus Status { get; init; }
    public bool CanBeCancelled { get; init; }

    public IEnumerable<OrderItemDto> Items { get; init; } = Enumerable.Empty<OrderItemDto>();

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
