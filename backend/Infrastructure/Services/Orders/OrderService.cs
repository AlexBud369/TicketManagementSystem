using Application.Common.Models;
using Application.DTOs.Orders;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Orders.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Services.Audit;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Infrastructure.Services.Orders;

public sealed class OrderService : IOrderService {
    private readonly AppDbContext _dbContext;
    private readonly IAppSettings _appSettings;
    private readonly IEmailService _emailService;
    private readonly StripeOptionsAdapter _stripe;
    private readonly AuditLogWriter _auditLogWriter;
    private readonly IMapper _mapper;

    public OrderService(
        AppDbContext dbContext,
        IAppSettings appSettings,
        IEmailService emailService,
        Microsoft.Extensions.Options.IOptions<Infrastructure.Settings.StripeOptions> stripeOptions,
        AuditLogWriter auditLogWriter,
        IMapper mapper) {
        _dbContext = dbContext;
        _appSettings = appSettings;
        _emailService = emailService;
        _stripe = new StripeOptionsAdapter(stripeOptions.Value);
        _auditLogWriter = auditLogWriter;
        _mapper = mapper;
    }

    public async Task<OrderDto> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default) {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var eventEntity = await _dbContext.Events
            .FirstOrDefaultAsync(item => item.Id == request.EventId, cancellationToken);

        if (eventEntity is null) {
            throw AppException.NotFound("Event", request.EventId);
        }

        if (eventEntity.AvailableTickets < request.Quantity) {
            throw AppException.BusinessRule(
                $"Only {eventEntity.AvailableTickets} tickets available.");
        }

        eventEntity.AvailableTickets -= request.Quantity;

        var unitPrice = eventEntity.TicketPrice;
        var subTotal = unitPrice * request.Quantity;
        var serviceFee = decimal.Round(
            subTotal * (decimal)_appSettings.ServiceFeePercentage,
            2,
            MidpointRounding.AwayFromZero);
        var total = subTotal + serviceFee;

        var order = new Order {
            OrderNumber = CreateOrderNumber(),
            UserId = request.UserId,
            EventId = request.EventId,
            Quantity = request.Quantity,
            SubTotal = subTotal,
            ServiceFee = serviceFee,
            Total = total,
            Status = OrderStatus.Pending
        };
        order.Id = Guid.NewGuid();

        for (var index = 0; index < request.Quantity; index++) {
            order.OrderItems.Add(new OrderItem {
                UnitPrice = unitPrice
            });
        }

        _dbContext.Orders.Add(order);
        _auditLogWriter.Append(
            AuditAction.OrderCreated,
            nameof(Order),
            order.Id.ToString(),
            request.UserId);

        try {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException) {
            await transaction.RollbackAsync(cancellationToken);
            throw AppException.BusinessRule(
                "Tickets were just purchased by someone else. Try a smaller quantity.");
        }

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task CancelAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default) {
        var order = await LoadTrackedOrderAsync(orderId, cancellationToken);

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded) {
            throw AppException.BusinessRule("This order cannot be cancelled.");
        }

        if (order.Status == OrderStatus.Pending) {
            await RestoreStockAsync(order, cancellationToken);
            order.Status = OrderStatus.Cancelled;
            _auditLogWriter.Append(
                AuditAction.OrderCancelled,
                nameof(Order),
                order.Id.ToString(),
                userId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (order.Status == OrderStatus.Paid) {
            await RefundStripeIfPossibleAsync(order, cancellationToken);
            await RestoreStockAsync(order, cancellationToken);
            MarkTickets(order, TicketStatus.Cancelled);
            SetPaymentRefunded(order);
            order.Status = OrderStatus.Cancelled;
            _auditLogWriter.Append(
                AuditAction.OrderCancelled,
                nameof(Order),
                order.Id.ToString(),
                userId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await SendRefundEmailAsync(order, cancellationToken);
        }
    }

    public async Task RefundAsync(
        Guid orderId,
        CancellationToken cancellationToken = default) {
        var order = await LoadTrackedOrderAsync(orderId, cancellationToken);

        if (order.Status != OrderStatus.Paid) {
            throw AppException.BusinessRule("Only paid orders can be refunded.");
        }

        await RefundStripeIfPossibleAsync(order, cancellationToken);
        await RestoreStockAsync(order, cancellationToken);
        MarkTickets(order, TicketStatus.Refunded);
        SetPaymentRefunded(order);
        order.Status = OrderStatus.Refunded;
        _auditLogWriter.Append(
            AuditAction.RefundIssued,
            nameof(Order),
            order.Id.ToString(),
            order.UserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await SendRefundEmailAsync(order, cancellationToken);
    }

    public async Task<OrderDto> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default) {
        var order = await QueryOrders()
            .FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null) {
            throw AppException.NotFound("Order", orderId);
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResult<OrderListDto>> GetMyOrdersAsync(
        Guid userId,
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default) {
        var query = ApplyFilter(
            QueryOrders().Where(order => order.UserId == userId),
            filter);

        return await PageAsync(query, filter, includeBuyer: false, cancellationToken);
    }

    public async Task<PagedResult<OrderListDto>> GetAllAsync(
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default) {
        var query = ApplyFilter(QueryOrders(), filter);
        return await PageAsync(query, filter, includeBuyer: true, cancellationToken);
    }

    public async Task<IReadOnlyCollection<EventBuyerDto>> GetPaidBuyersByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default) {
        return await _mapper.ProjectTo<EventBuyerDto>(
                _dbContext.Orders
                    .AsNoTracking()
                    .Where(order =>
                        order.EventId == eventId &&
                        order.Status == OrderStatus.Paid))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Order> QueryOrders() {
        return _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.EventEntity)!
                .ThenInclude(eventEntity => eventEntity.BannerImage)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Ticket);
    }

    private static IQueryable<Order> ApplyFilter(
        IQueryable<Order> query,
        OrderFilterRequest filter) {
        if (!string.IsNullOrWhiteSpace(filter.Search)) {
            var term = filter.Search.Trim();
            query = query.Where(order =>
                order.OrderNumber.Contains(term) ||
                (order.EventEntity != null && order.EventEntity.Title.Contains(term)) ||
                (order.User != null &&
                    (order.User.FirstName.Contains(term) ||
                     order.User.LastName.Contains(term) ||
                     order.User.Email.Contains(term))));
        }

        if (Enum.TryParse<OrderStatus>(filter.StatusFilter, true, out var status)) {
            query = query.Where(order => order.Status == status);
        }

        if (filter.DateFrom.HasValue) {
            query = query.Where(order => order.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue) {
            query = query.Where(order => order.CreatedAt <= filter.DateTo.Value);
        }

        return query.OrderByDescending(order => order.CreatedAt);
    }

    private async Task<PagedResult<OrderListDto>> PageAsync(
        IQueryable<Order> query,
        OrderFilterRequest filter,
        bool includeBuyer,
        CancellationToken cancellationToken) {
        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<OrderListDto>>(orders);
        if (!includeBuyer) {
            items = items
                .Select(item => new OrderListDto {
                    Id = item.Id,
                    OrderNumber = item.OrderNumber,
                    EventTitle = item.EventTitle,
                    EventDate = item.EventDate,
                    BuyerName = null,
                    Quantity = item.Quantity,
                    Total = item.Total,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt
                })
                .ToList();
        }

        return PagedResult.Create(items, totalCount, filter.Page, filter.PageSize);
    }

    private async Task<Order> LoadTrackedOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken) {
        var order = await _dbContext.Orders
            .Include(item => item.User)
            .Include(item => item.Payment)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Ticket)
            .Include(item => item.EventEntity)
            .FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);

        if (order is null) {
            throw AppException.NotFound("Order", orderId);
        }

        return order;
    }

    private async Task RestoreStockAsync(Order order, CancellationToken cancellationToken) {
        var eventEntity = await _dbContext.Events
            .FirstOrDefaultAsync(item => item.Id == order.EventId, cancellationToken);

        if (eventEntity is null) {
            return;
        }

        eventEntity.AvailableTickets += order.Quantity;
        if (eventEntity.AvailableTickets > eventEntity.MaxCapacity) {
            eventEntity.AvailableTickets = eventEntity.MaxCapacity;
        }
    }

    private async Task RefundStripeIfPossibleAsync(
        Order order,
        CancellationToken cancellationToken) {
        var paymentIntentId = order.Payment?.StripePaymentIntentId;
        if (string.IsNullOrWhiteSpace(paymentIntentId) || !_stripe.IsConfigured) {
            return;
        }

        try {
            var refundService = new RefundService(_stripe.Client);
            await refundService.CreateAsync(
                new RefundCreateOptions { PaymentIntent = paymentIntentId },
                cancellationToken: cancellationToken);
        }
        catch (StripeException exception) {
            throw AppException.BusinessRule(
                $"Stripe refund failed: {exception.Message}");
        }
    }

    private static void MarkTickets(Order order, TicketStatus status) {
        foreach (var item in order.OrderItems) {
            if (item.Ticket is not null) {
                item.Ticket.Status = status;
            }
        }
    }

    private static void SetPaymentRefunded(Order order) {
        if (order.Payment is null) {
            return;
        }

        order.Payment.Status = PaymentStatus.Refunded;
        order.Payment.RefundedAt = DateTime.UtcNow;
    }

    private async Task SendRefundEmailAsync(Order order, CancellationToken cancellationToken) {
        if (order.User is null) {
            return;
        }

        await _emailService.SendRefundConfirmationEmailAsync(
            order.User.Email,
            $"{order.User.FirstName} {order.User.LastName}".Trim(),
            order.OrderNumber,
            order.Total,
            cancellationToken);
    }

    private static string CreateOrderNumber() {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }

    private sealed class StripeOptionsAdapter {
        public StripeOptionsAdapter(Infrastructure.Settings.StripeOptions options) {
            IsConfigured = !string.IsNullOrWhiteSpace(options.SecretKey);
            Client = IsConfigured ? new StripeClient(options.SecretKey) : null!;
        }

        public bool IsConfigured { get; }
        public StripeClient Client { get; }
    }
}
