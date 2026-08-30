using Application.Interfaces;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services.Email;

public sealed class SmtpEmailService : IEmailService {
    private readonly EmailOptions _options;

    public SmtpEmailService(IOptions<EmailOptions> options) {
        _options = options.Value;
    }

    public Task SendRegistrationEmailAsync(
        string toEmail,
        string userName,
        string confirmationLink,
        CancellationToken cancellationToken = default) {
        return SendAsync(
            toEmail,
            "Confirm your email",
            $"Hello {userName},\n\nConfirm your account:\n{confirmationLink}\n",
            cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken = default) {
        return SendAsync(
            toEmail,
            "Reset your password",
            $"Reset your password using this link:\n{resetLink}\n",
            cancellationToken);
    }

    public Task SendOrderConfirmationEmailAsync(
        string toEmail,
        string userName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken cancellationToken = default) {
        return SendAsync(
            toEmail,
            $"Order {orderNumber} confirmed",
            $"Hello {userName},\n\nYour order {orderNumber} is paid. Total: {totalAmount:0.00}.\n",
            cancellationToken);
    }

    public Task SendRefundConfirmationEmailAsync(
        string toEmail,
        string userName,
        string orderNumber,
        decimal refundAmount,
        CancellationToken cancellationToken = default) {
        return SendAsync(
            toEmail,
            $"Refund for order {orderNumber}",
            $"Hello {userName},\n\nA refund of {refundAmount:0.00} was issued for order {orderNumber}.\n",
            cancellationToken);
    }

    public Task SendEventCancellationEmailAsync(
        string toEmail,
        string userName,
        string eventTitle,
        CancellationToken cancellationToken = default) {
        return SendAsync(
            toEmail,
            $"Event cancelled: {eventTitle}",
            $"Hello {userName},\n\nThe event \"{eventTitle}\" was cancelled.\n",
            cancellationToken);
    }

    private async Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.From)) {
            throw new InvalidOperationException(
                "Email host and from address must be configured to send mail.");
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            ResolveSocketOptions(),
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.User)) {
            await client.AuthenticateAsync(
                _options.User,
                _options.Password,
                cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private SecureSocketOptions ResolveSocketOptions() {
        if (_options.Port == 465) {
            return SecureSocketOptions.SslOnConnect;
        }

        return _options.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;
    }
}
