namespace Application.Interfaces;

public interface IPaymentService {
    Task<string> CreateCheckoutSessionAsync(
        Guid orderId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms payment, generates tickets, and sends the confirmation email.
    /// Duplicate Stripe event ids must be ignored (see Payment.StripeEventId).
    /// </summary>
    Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);
}
