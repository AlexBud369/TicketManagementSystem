namespace Application.Interfaces.Orders.Models;

public sealed class CreateOrderRequest {
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
}
