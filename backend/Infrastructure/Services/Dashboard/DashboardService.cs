using System.Globalization;
using Application.DTOs.Dashboard;
using Application.Interfaces;
using Domain.Enums;
using Domain.Scheduling;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Dashboard;

public sealed class DashboardService : IDashboardService {
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(
        CancellationToken cancellationToken = default) {
        var now = DateTime.UtcNow;

        var totalUsers = await _dbContext.AppUsers.CountAsync(cancellationToken);
        var activeUsers = await _dbContext.AppUsers
            .CountAsync(user => user.Status == UserStatus.Active, cancellationToken);
        var disabledUsers = await _dbContext.AppUsers
            .CountAsync(user => user.Status == UserStatus.Disabled, cancellationToken);

        var totalEvents = await _dbContext.Events.CountAsync(cancellationToken);
        var publishedEvents = await _dbContext.Events
            .CountAsync(eventEntity => eventEntity.IsPublished, cancellationToken);

        var published = await _dbContext.Events
            .AsNoTracking()
            .Where(eventEntity => eventEntity.IsPublished)
            .Select(eventEntity => new { eventEntity.Date, eventEntity.StartTime })
            .ToListAsync(cancellationToken);

        var upcomingEvents = published.Count(item =>
            !EventSchedule.HasStarted(item.Date, item.StartTime, now));

        var paidOrders = _dbContext.Orders.Where(order => order.Status == OrderStatus.Paid);
        var refundedOrders = _dbContext.Orders.Where(order => order.Status == OrderStatus.Refunded);

        var ticketsSold = await paidOrders.SumAsync(order => (int?)order.Quantity, cancellationToken) ?? 0;
        var totalRevenue = await paidOrders.SumAsync(order => (decimal?)order.Total, cancellationToken) ?? 0m;
        var totalRefunds = await refundedOrders.CountAsync(cancellationToken);
        var refundedAmount = await refundedOrders.SumAsync(order => (decimal?)order.Total, cancellationToken) ?? 0m;

        return new DashboardStatsDto {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            DisabledUsers = disabledUsers,
            TotalEvents = totalEvents,
            PublishedEvents = publishedEvents,
            UpcomingEvents = upcomingEvents,
            TicketsSold = ticketsSold,
            TotalRevenue = totalRevenue,
            TotalRefunds = totalRefunds,
            RefundedAmount = refundedAmount,
            MonthlySales = await GetMonthlySalesAsync(now, cancellationToken),
            RevenueByEvent = await GetRevenueByEventAsync(cancellationToken),
            TopEvents = await GetTopEventsAsync(cancellationToken),
            GeneratedAt = now
        };
    }

    private async Task<IReadOnlyCollection<MonthlySalesDto>> GetMonthlySalesAsync(
        DateTime utcNow,
        CancellationToken cancellationToken) {
        var from = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-11);

        var rows = await _dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.Status == OrderStatus.Paid &&
                order.CreatedAt >= from)
            .GroupBy(order => new { order.CreatedAt.Year, order.CreatedAt.Month })
            .Select(group => new {
                group.Key.Year,
                group.Key.Month,
                TicketsSold = group.Sum(order => order.Quantity),
                Revenue = group.Sum(order => order.Total)
            })
            .ToListAsync(cancellationToken);

        var byMonth = rows.ToDictionary(
            row => (row.Year, row.Month),
            row => row);

        var result = new List<MonthlySalesDto>(12);
        for (var offset = 0; offset < 12; offset++) {
            var monthDate = from.AddMonths(offset);
            byMonth.TryGetValue((monthDate.Year, monthDate.Month), out var row);

            result.Add(new MonthlySalesDto {
                Year = monthDate.Year,
                Month = monthDate.Month,
                MonthName = monthDate.ToString("MMMM", CultureInfo.InvariantCulture),
                TicketsSold = row?.TicketsSold ?? 0,
                Revenue = row?.Revenue ?? 0m
            });
        }

        return result;
    }

    private async Task<IReadOnlyCollection<RevenueByEventDto>> GetRevenueByEventAsync(
        CancellationToken cancellationToken) {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Status == OrderStatus.Paid)
            .GroupBy(order => new {
                order.EventId,
                Title = order.EventEntity != null ? order.EventEntity.Title : string.Empty
            })
            .Select(group => new RevenueByEventDto {
                EventId = group.Key.EventId,
                EventTitle = group.Key.Title,
                Revenue = group.Sum(order => order.Total),
                TicketsSold = group.Sum(order => order.Quantity)
            })
            .OrderByDescending(item => item.Revenue)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<TopEventDto>> GetTopEventsAsync(
        CancellationToken cancellationToken) {
        return await _dbContext.Events
            .AsNoTracking()
            .Select(eventEntity => new TopEventDto {
                EventId = eventEntity.Id,
                EventTitle = eventEntity.Title,
                BannerImageUrl = eventEntity.BannerImage != null
                    ? eventEntity.BannerImage.Url
                    : null,
                EventDate = eventEntity.Date,
                TicketsSold = eventEntity.MaxCapacity - eventEntity.AvailableTickets,
                MaxCapacity = eventEntity.MaxCapacity,
                Revenue = eventEntity.Orders
                    .Where(order => order.Status == OrderStatus.Paid)
                    .Sum(order => (decimal?)order.Total) ?? 0m,
                OccupancyRate = eventEntity.MaxCapacity == 0
                    ? 0
                    : (double)(eventEntity.MaxCapacity - eventEntity.AvailableTickets)
                        / eventEntity.MaxCapacity
            })
            .OrderByDescending(item => item.TicketsSold)
            .ThenByDescending(item => item.Revenue)
            .Take(5)
            .ToListAsync(cancellationToken);
    }
}
