namespace Presentation.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid EventId,
    int Quantity);
