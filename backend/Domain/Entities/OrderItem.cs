using Domain.Common;

namespace Domain.Entities;

public class OrderItem : BaseEntity {
    public Guid OrderId { get; set; }
    public Guid? TicketId { get; set; }
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
    public Ticket? Ticket { get; set; }
}
