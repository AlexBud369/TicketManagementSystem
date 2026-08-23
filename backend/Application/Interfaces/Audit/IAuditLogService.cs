using Application.Common.Models;
using Application.DTOs.Audit;

namespace Application.Interfaces;

public interface IAuditLogService {
    Task<PagedResult<AuditLogDto>> SearchAsync(
        string search,
        string actionFilter,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
