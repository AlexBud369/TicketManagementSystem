using Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts.Payments;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase {
    private readonly ISender _sender;

    public PaymentsController(ISender sender) {
        _sender = sender;
    }

    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreatePaymentSessionRequest request,
        CancellationToken cancellationToken) {
        var url = await _sender.Send(
            new CreatePaymentSessionCommand(
                request.OrderId,
                User.GetUserId()),
            cancellationToken);

        return Ok(new { url });
    }
}
