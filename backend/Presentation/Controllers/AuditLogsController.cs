using Application.Features.Audit.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;

namespace Presentation.Controllers;

[Authorize(Roles = AppRoleNames.Admin)]
[ApiController]
[Route("api/admin/audit-logs")]
public sealed class AuditLogsController : ControllerBase {
    private readonly ISender _sender;

    public AuditLogsController(ISender sender) {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? actionFilter,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) {
        var result = await _sender.Send(
            new GetAuditLogsQuery(
                search ?? string.Empty,
                actionFilter ?? string.Empty,
                dateFrom,
                dateTo,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }
}
