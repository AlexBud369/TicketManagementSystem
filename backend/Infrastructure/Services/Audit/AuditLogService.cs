using Application.Common.Models;
using Application.DTOs.Audit;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Audit;

public sealed class AuditLogService : IAuditLogService {
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public AuditLogService(AppDbContext dbContext, IMapper mapper) {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDto>> SearchAsync(
        string search,
        string actionFilter,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) {
        var query = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim();
            query = query.Where(log =>
                log.EntityName.Contains(term) ||
                (log.EntityId != null && log.EntityId.Contains(term)) ||
                (log.User != null && log.User.Email.Contains(term)));
        }

        if (Enum.TryParse<AuditAction>(actionFilter, true, out var action)) {
            query = query.Where(log => log.Action == action);
        }

        if (dateFrom.HasValue) {
            query = query.Where(log => log.CreatedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue) {
            query = query.Where(log => log.CreatedAt <= dateTo.Value);
        }

        query = query.OrderByDescending(log => log.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await _mapper.ProjectTo<AuditLogDto>(query)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult.Create(items, totalCount, page, pageSize);
    }
}
