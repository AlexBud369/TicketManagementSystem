using Application.Features.Tickets.Commands;
using Application.Features.Tickets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;
using Presentation.Contracts.Tickets;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase {
    private readonly ISender _sender;

    public TicketsController(ISender sender) {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetMyTicketsQuery(User.GetUserId()),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetTicketByIdQuery(
                id,
                User.GetUserId(),
                User.IsAdmin()),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(
        Guid id,
        CancellationToken cancellationToken) {
        var pdf = await _sender.Send(
            new DownloadTicketQuery(
                id,
                User.GetUserId(),
                User.IsAdmin()),
            cancellationToken);

        return File(pdf, "application/pdf", $"ticket-{id}.pdf");
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate(
        [FromBody] ValidateTicketRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new ValidateTicketCommand(request.QrCode),
            cancellationToken);

        return Ok(result);
    }
}
