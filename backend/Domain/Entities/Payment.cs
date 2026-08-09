using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Payment : BaseEntity {
    public Guid OrderId { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    public Order? Order { get; set; }
}
