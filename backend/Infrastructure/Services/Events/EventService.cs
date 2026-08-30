using Application.Common.Models;
using Application.DTOs.Events;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Events.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Events;

public sealed class EventService : IEventService {
    private readonly AppDbContext _dbContext;
    private readonly AuditLogWriter _auditLogWriter;
    private readonly IMapper _mapper;

    public EventService(
        AppDbContext dbContext,
        AuditLogWriter auditLogWriter,
        IMapper mapper) {
        _dbContext = dbContext;
        _auditLogWriter = auditLogWriter;
        _mapper = mapper;
    }

    public async Task<EventDto> CreateAsync(
        CreateEventRequest request,
        CancellationToken cancellationToken = default) {
        var organizerExists = await _dbContext.AppUsers
            .AnyAsync(user => user.Id == request.OrganizerId, cancellationToken);

        if (!organizerExists) {
            throw AppException.NotFound("User", request.OrganizerId);
        }

        var eventEntity = new EventEntity {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            OrganizerId = request.OrganizerId,
            Location = request.Location.Trim(),
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxCapacity = request.MaximumCapacity,
            AvailableTickets = request.MaximumCapacity,
            TicketPrice = request.TicketPrice,
            IsPublished = false,
            AllowCancellation = request.AllowCancellation,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        eventEntity.Id = Guid.NewGuid();
        _dbContext.Events.Add(eventEntity);
        _auditLogWriter.Append(
            AuditAction.EventCreated,
            nameof(EventEntity),
            eventEntity.Id.ToString(),
            request.OrganizerId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(eventEntity.Id, cancellationToken);
    }

    public async Task<EventDto> UpdateAsync(
        Guid eventId,
        UpdateEventRequest request,
        CancellationToken cancellationToken = default) {
        var eventEntity = await RequireAsync(eventId, cancellationToken);

        var soldTickets = eventEntity.MaxCapacity - eventEntity.AvailableTickets;
        if (request.MaximumCapacity < soldTickets) {
            throw AppException.BusinessRule(
                "Capacity cannot be lower than the number of tickets already sold.");
        }

        eventEntity.Title = request.Title.Trim();
        eventEntity.Description = request.Description.Trim();
        eventEntity.CategoryId = request.CategoryId;
        eventEntity.Location = request.Location.Trim();
        eventEntity.Date = request.Date;
        eventEntity.StartTime = request.StartTime;
        eventEntity.EndTime = request.EndTime;
        eventEntity.TicketPrice = request.TicketPrice;
        eventEntity.AllowCancellation = request.AllowCancellation;
        eventEntity.Latitude = request.Latitude;
        eventEntity.Longitude = request.Longitude;
        eventEntity.MaxCapacity = request.MaximumCapacity;
        eventEntity.AvailableTickets = request.MaximumCapacity - soldTickets;

        _auditLogWriter.Append(
            AuditAction.EventUpdated,
            nameof(EventEntity),
            eventId.ToString(),
            eventEntity.OrganizerId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(eventId, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid eventId,
        CancellationToken cancellationToken = default) {
        var eventEntity = await RequireAsync(eventId, cancellationToken);

        var hasOrders = await _dbContext.Orders
            .AnyAsync(order => order.EventId == eventId, cancellationToken);

        if (hasOrders) {
            throw AppException.BusinessRule(
                "Cannot delete an event that already has orders.");
        }

        _dbContext.Events.Remove(eventEntity);
        _auditLogWriter.Append(
            AuditAction.EventDeleted,
            nameof(EventEntity),
            eventId.ToString(),
            eventEntity.OrganizerId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishAsync(
        Guid eventId,
        CancellationToken cancellationToken = default) {
        var eventEntity = await RequireAsync(eventId, cancellationToken);
        eventEntity.IsPublished = true;
        _auditLogWriter.Append(
            AuditAction.EventPublished,
            nameof(EventEntity),
            eventId.ToString(),
            eventEntity.OrganizerId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishAsync(
        Guid eventId,
        CancellationToken cancellationToken = default) {
        var eventEntity = await RequireAsync(eventId, cancellationToken);
        eventEntity.IsPublished = false;
        _auditLogWriter.Append(
            AuditAction.EventUnpublished,
            nameof(EventEntity),
            eventId.ToString(),
            eventEntity.OrganizerId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EventDto> GetByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default) {
        var dto = await QueryDetails()
            .FirstOrDefaultAsync(eventEntity => eventEntity.Id == eventId, cancellationToken);

        if (dto is null) {
            throw AppException.NotFound("Event", eventId);
        }

        return dto;
    }

    public async Task<PagedResult<EventListDto>> GetAllAsync(
        EventFilterRequest filter,
        CancellationToken cancellationToken = default) {
        var query = _dbContext.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search)) {
            var term = filter.Search.Trim();
            query = query.Where(eventEntity =>
                eventEntity.Title.Contains(term) ||
                eventEntity.Location.Contains(term));
        }

        if (filter.CategoryId.HasValue) {
            query = query.Where(eventEntity => eventEntity.CategoryId == filter.CategoryId.Value);
        }

        if (filter.DateFrom.HasValue) {
            query = query.Where(eventEntity => eventEntity.Date >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue) {
            query = query.Where(eventEntity => eventEntity.Date <= filter.DateTo.Value);
        }

        if (filter.MinPrice.HasValue) {
            query = query.Where(eventEntity => eventEntity.TicketPrice >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue) {
            query = query.Where(eventEntity => eventEntity.TicketPrice <= filter.MaxPrice.Value);
        }

        if (filter.HasAvailableTickets == true) {
            query = query.Where(eventEntity => eventEntity.AvailableTickets > 0);
        }

        query = ApplySort(query, filter.SortBy, filter.Descending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await _mapper.ProjectTo<EventListDto>(query)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult.Create(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task UpdateBannerAsync(
        Guid eventId,
        Guid imageId,
        CancellationToken cancellationToken = default) {
        var eventEntity = await RequireAsync(eventId, cancellationToken);

        var imageExists = await _dbContext.Images
            .AnyAsync(image => image.Id == imageId, cancellationToken);

        if (!imageExists) {
            throw AppException.NotFound("Image", imageId);
        }

        eventEntity.BannerImageId = imageId;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasPaidOrdersAsync(
        Guid eventId,
        CancellationToken cancellationToken = default) {
        return await _dbContext.Orders
            .AnyAsync(
                order => order.EventId == eventId && order.Status == OrderStatus.Paid,
                cancellationToken);
    }

    private async Task<EventEntity> RequireAsync(
        Guid eventId,
        CancellationToken cancellationToken) {
        var eventEntity = await _dbContext.Events
            .FirstOrDefaultAsync(item => item.Id == eventId, cancellationToken);

        if (eventEntity is null) {
            throw AppException.NotFound("Event", eventId);
        }

        return eventEntity;
    }

    private IQueryable<EventDto> QueryDetails() {
        return _mapper.ProjectTo<EventDto>(_dbContext.Events.AsNoTracking());
    }

    private static IQueryable<EventEntity> ApplySort(
        IQueryable<EventEntity> query,
        string sortBy,
        bool descending) {
        return sortBy.Trim().ToLowerInvariant() switch {
            "price" => descending
                ? query.OrderByDescending(eventEntity => eventEntity.TicketPrice)
                : query.OrderBy(eventEntity => eventEntity.TicketPrice),
            "title" => descending
                ? query.OrderByDescending(eventEntity => eventEntity.Title)
                : query.OrderBy(eventEntity => eventEntity.Title),
            "created" => descending
                ? query.OrderByDescending(eventEntity => eventEntity.CreatedAt)
                : query.OrderBy(eventEntity => eventEntity.CreatedAt),
            _ => descending
                ? query.OrderByDescending(eventEntity => eventEntity.Date)
                    .ThenByDescending(eventEntity => eventEntity.StartTime)
                : query.OrderBy(eventEntity => eventEntity.Date)
                    .ThenBy(eventEntity => eventEntity.StartTime)
        };
    }
}
