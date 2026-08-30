using Application.DTOs.Reports;
using Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;

namespace Presentation.Controllers;

[Authorize(Roles = AppRoleNames.Admin)]
[ApiController]
[Route("api/admin/reports")]
public sealed class ReportsController : ControllerBase {
    private readonly ISender _sender;

    public ReportsController(ISender sender) {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Export(
        [FromQuery] ReportType type,
        [FromQuery] ReportFormat format,
        CancellationToken cancellationToken) {
        var file = await _sender.Send(
            new ExportReportQuery(type, format),
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }
}
