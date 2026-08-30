using Application.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;

namespace Presentation.Controllers;

[Authorize(Roles = AppRoleNames.Admin)]
[ApiController]
[Route("api/admin/dashboard")]
public sealed class DashboardController : ControllerBase {
    private readonly ISender _sender;

    public DashboardController(ISender sender) {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetDashboardStatsQuery(),
            cancellationToken);

        return Ok(result);
    }
}
