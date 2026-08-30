using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Email;

public sealed class LoggingEmailService : IEmailService {
    private readonly ILogger<LoggingEmailService> _logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger) {
        _logger = logger;
    }

    public Task SendRegistrationEmailAsync(
        string toEmail,
        string userName,
        string confirmationLink,
        CancellationToken cancellationToken = default) {
        _logger.LogInformation(
            "Registration email for {UserName} <{Email}>. Confirmation link: {Link}",
            userName,
            toEmail,
            confirmationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken = default) {
        _logger.LogInformation(
            "Password reset email for <{Email}>. Reset link: {Link}",
            toEmail,
            resetLink);

        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationEmailAsync(
        string toEmail,
        string userName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken cancellationToken = default) {
        _logger.LogInformation(
            "Order confirmation for {UserName} <{Email}>. Order {OrderNumber}, total {Total}",
            userName,
            toEmail,
            orderNumber,
            totalAmount);

        return Task.CompletedTask;
    }

    public Task SendRefundConfirmationEmailAsync(
        string toEmail,
        string userName,
        string orderNumber,
        decimal refundAmount,
        CancellationToken cancellationToken = default) {
        _logger.LogInformation(
            "Refund email for {UserName} <{Email}>. Order {OrderNumber}, refund {Amount}",
            userName,
            toEmail,
            orderNumber,
            refundAmount);

        return Task.CompletedTask;
    }

    public Task SendEventCancellationEmailAsync(
        string toEmail,
        string userName,
        string eventTitle,
        CancellationToken cancellationToken = default) {
        _logger.LogInformation(
            "Event cancellation email for {UserName} <{Email}>. Event {EventTitle}",
            userName,
            toEmail,
            eventTitle);

        return Task.CompletedTask;
    }
}
