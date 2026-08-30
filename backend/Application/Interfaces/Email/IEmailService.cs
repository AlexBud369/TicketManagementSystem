namespace Application.Interfaces;

public interface IEmailService {
    Task SendRegistrationEmailAsync(
        string toEmail,
        string userName,
        string confirmationLink,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken = default);

    Task SendOrderConfirmationEmailAsync(
        string toEmail,
        string userName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken cancellationToken = default);

    Task SendRefundConfirmationEmailAsync(
        string toEmail,
        string userName,
        string orderNumber,
        decimal refundAmount,
        CancellationToken cancellationToken = default);

    Task SendEventCancellationEmailAsync(
        string toEmail,
        string userName,
        string eventTitle,
        CancellationToken cancellationToken = default);
}
