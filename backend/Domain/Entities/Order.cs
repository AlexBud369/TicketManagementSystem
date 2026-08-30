using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Order : BaseEntity {
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public User? User { get; set; }
    public EventEntity? EventEntity { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}
