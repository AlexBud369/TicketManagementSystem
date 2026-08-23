using Application.Common.Models;
using Application.DTOs.Audit;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Audit.Queries;

public sealed record GetAuditLogsQuery(
    string Search,
    string ActionFilter,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page,
    int PageSize
) : IRequest<PagedResult<AuditLogDto>>;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>> {
    private readonly IAuditLogService _auditLogService;

    public GetAuditLogsQueryHandler(IAuditLogService auditLogService) {
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken) {
        var pagination = new PaginationParams {
            Page = request.Page,
            PageSize = request.PageSize
        };

        return await _auditLogService.SearchAsync(
            request.Search,
            request.ActionFilter,
            request.DateFrom,
            request.DateTo,
            pagination.Page,
            pagination.PageSize,
            cancellationToken);
    }
}
