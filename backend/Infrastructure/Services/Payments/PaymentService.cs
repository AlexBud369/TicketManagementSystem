using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Services.Audit;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services.Payments;

public sealed class PaymentService : IPaymentService {
    private const string OrderIdMetadataKey = "orderId";

    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly StripeOptions _stripeOptions;
    private readonly AuditLogWriter _auditLogWriter;

    public PaymentService(
        AppDbContext dbContext,
        IEmailService emailService,
        IOptions<StripeOptions> stripeOptions,
        AuditLogWriter auditLogWriter) {
        _dbContext = dbContext;
        _emailService = emailService;
        _stripeOptions = stripeOptions.Value;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        Guid orderId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default) {
        EnsureStripeConfigured();

        var order = await _dbContext.Orders
            .Include(item => item.User)
            .Include(item => item.EventEntity)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null) {
            throw AppException.NotFound("Order", orderId);
        }

        if (order.Status != OrderStatus.Pending) {
            throw AppException.BusinessRule("Only pending orders can be paid.");
        }

        var currency = string.IsNullOrWhiteSpace(_stripeOptions.Currency)
            ? "usd"
            : _stripeOptions.Currency.ToLowerInvariant();

        var amountCents = ToCents(order.Total);
        var client = new StripeClient(_stripeOptions.SecretKey);
        var sessionService = new SessionService(client);

        var session = await sessionService.CreateAsync(
            new SessionCreateOptions {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = order.User?.Email,
                ClientReferenceId = order.Id.ToString(),
                Metadata = new Dictionary<string, string> {
                    [OrderIdMetadataKey] = order.Id.ToString()
                },
                LineItems = [
                    new SessionLineItemOptions {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions {
                            Currency = currency,
                            UnitAmount = amountCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions {
                                Name = order.EventEntity?.Title ?? "Event tickets",
                                Description = $"Order {order.OrderNumber} ({order.Quantity} tickets)"
                            }
                        }
                    }
                ]
            },
            cancellationToken: cancellationToken);

        if (order.Payment is null) {
            order.Payment = new Payment {
                OrderId = order.Id,
                StripeSessionId = session.Id,
                Amount = order.Total,
                Currency = currency,
                Status = PaymentStatus.Pending
            };
        }
        else {
            order.Payment.StripeSessionId = session.Id;
            order.Payment.Amount = order.Total;
            order.Payment.Currency = currency;
            order.Payment.Status = PaymentStatus.Pending;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(session.Url)) {
            throw AppException.BusinessRule("Stripe did not return a checkout URL.");
        }

        return session.Url;
    }

    public async Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default) {
        EnsureStripeConfigured();

        if (string.IsNullOrWhiteSpace(_stripeOptions.WebhookSecret)) {
            throw AppException.BusinessRule(
                "Stripe:WebhookSecret is not configured.");
        }

        Event stripeEvent;
        try {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _stripeOptions.WebhookSecret);
        }
        catch (StripeException) {
            throw AppException.Unauthorized("Invalid Stripe webhook signature.");
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted) {
            await FulfillCheckoutAsync(stripeEvent, cancellationToken);
            return;
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionExpired) {
            await ReleaseExpiredCheckoutAsync(stripeEvent, cancellationToken);
        }
    }

    private async Task FulfillCheckoutAsync(
        Event stripeEvent,
        CancellationToken cancellationToken) {
        if (stripeEvent.Data.Object is not Session session) {
            return;
        }

        var alreadyProcessed = await _dbContext.Payments
            .AnyAsync(
                payment => payment.StripeEventId == stripeEvent.Id,
                cancellationToken);

        if (alreadyProcessed) {
            return;
        }

        var orderId = ResolveOrderId(session);
        if (orderId is null) {
            throw AppException.NotFound("Order id was missing from the Stripe session.");
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var order = await _dbContext.Orders
            .Include(item => item.User)
            .Include(item => item.Payment)
            .Include(item => item.OrderItems)
            .FirstOrDefaultAsync(item => item.Id == orderId.Value, cancellationToken);

        if (order is null) {
            throw AppException.NotFound("Order", orderId.Value);
        }

        if (order.Payment is not null) {
            order.Payment.StripeEventId = stripeEvent.Id;
            order.Payment.StripePaymentIntentId =
                session.PaymentIntentId ?? order.Payment.StripePaymentIntentId;
        }

        if (order.Status == OrderStatus.Paid) {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        IssueTickets(order);

        order.Status = OrderStatus.Paid;

        if (order.Payment is not null) {
            order.Payment.Status = PaymentStatus.Completed;
            order.Payment.PaidAt = DateTime.UtcNow;
            order.Payment.StripeSessionId = session.Id;
        }

        _auditLogWriter.Append(
            AuditAction.PaymentCompleted,
            nameof(Order),
            order.Id.ToString(),
            order.UserId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (order.User is not null) {
            await _emailService.SendOrderConfirmationEmailAsync(
                order.User.Email,
                $"{order.User.FirstName} {order.User.LastName}".Trim(),
                order.OrderNumber,
                order.Total,
                cancellationToken);
        }
    }

    private async Task ReleaseExpiredCheckoutAsync(
        Event stripeEvent,
        CancellationToken cancellationToken) {
        if (stripeEvent.Data.Object is not Session session) {
            return;
        }

        var orderId = ResolveOrderId(session);
        if (orderId is null) {
            return;
        }

        var order = await _dbContext.Orders
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == orderId.Value, cancellationToken);

        if (order is null || order.Status != OrderStatus.Pending) {
            return;
        }

        var eventEntity = await _dbContext.Events
            .FirstOrDefaultAsync(item => item.Id == order.EventId, cancellationToken);

        if (eventEntity is not null) {
            eventEntity.AvailableTickets += order.Quantity;
            if (eventEntity.AvailableTickets > eventEntity.MaxCapacity) {
                eventEntity.AvailableTickets = eventEntity.MaxCapacity;
            }
        }

        order.Status = OrderStatus.Cancelled;

        if (order.Payment is not null) {
            order.Payment.Status = PaymentStatus.Failed;
            order.Payment.StripeEventId = stripeEvent.Id;
        }

        _auditLogWriter.Append(
            AuditAction.PaymentFailed,
            nameof(Order),
            order.Id.ToString(),
            order.UserId);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    internal static void IssueTickets(Order order) {
        foreach (var item in order.OrderItems) {
            if (item.TicketId.HasValue) {
                continue;
            }

            var ticket = new Ticket {
                TicketNumber = CreateTicketNumber(),
                OrderId = order.Id,
                EventId = order.EventId,
                UserId = order.UserId,
                QrCode = Guid.NewGuid().ToString("N"),
                Status = TicketStatus.Active
            };

            item.Ticket = ticket;
        }
    }

    private static Guid? ResolveOrderId(Session session) {
        if (session.Metadata is not null &&
            session.Metadata.TryGetValue(OrderIdMetadataKey, out var metadataValue) &&
            Guid.TryParse(metadataValue, out var fromMetadata)) {
            return fromMetadata;
        }

        if (Guid.TryParse(session.ClientReferenceId, out var fromReference)) {
            return fromReference;
        }

        return null;
    }

    private void EnsureStripeConfigured() {
        if (string.IsNullOrWhiteSpace(_stripeOptions.SecretKey)) {
            throw AppException.BusinessRule(
                "Stripe:SecretKey is not configured.");
        }
    }

    private static long ToCents(decimal amount) {
        return (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
    }

    private static string CreateTicketNumber() {
        return $"TCK-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }
}
