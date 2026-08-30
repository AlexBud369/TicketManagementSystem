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
        WriteMailLink("Confirmation link", confirmationLink);
        WriteConfirmEmailJson(confirmationLink);

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
        WriteMailLink("Reset link", resetLink);
        WriteResetPasswordJson(resetLink);

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

    private static void WriteMailLink(string label, string link) {
        Console.Error.WriteLine($"[MAIL] {label}: {link}");
    }

    private static void WriteConfirmEmailJson(string confirmationLink) {
        if (!TryReadQuery(confirmationLink, out var email, out var token)) {
            return;
        }

        Console.Error.WriteLine(
            $"[MAIL] POST /api/auth/confirm-email JSON: {{\"email\":\"{email}\",\"token\":\"{token}\"}}");
    }

    private static void WriteResetPasswordJson(string resetLink) {
        if (!TryReadQuery(resetLink, out var email, out var token)) {
            return;
        }

        Console.Error.WriteLine(
            $"[MAIL] POST /api/auth/reset-password JSON: {{\"email\":\"{email}\",\"token\":\"{token}\",\"newPassword\":\"YourNewPassword1!\",\"confirmNewPassword\":\"YourNewPassword1!\"}}");
    }

    private static bool TryReadQuery(
        string link,
        out string email,
        out string token) {
        email = string.Empty;
        token = string.Empty;

        const string tokenKey = "token=";
        const string emailKey = "&email=";
        var tokenStart = link.IndexOf(tokenKey, StringComparison.Ordinal);
        var emailStart = link.IndexOf(emailKey, StringComparison.Ordinal);
        if (tokenStart < 0 || emailStart < 0 || emailStart <= tokenStart) {
            return false;
        }

        token = Uri.UnescapeDataString(
            link[(tokenStart + tokenKey.Length)..emailStart]);
        email = Uri.UnescapeDataString(link[(emailStart + emailKey.Length)..]);
        return token.Length > 0 && email.Length > 0;
    }
}
