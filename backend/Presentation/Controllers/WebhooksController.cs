using System.Text;
using Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController : ControllerBase {
    private readonly ISender _sender;

    public WebhooksController(ISender sender) {
        _sender = sender;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> Stripe(CancellationToken cancellationToken) {
        using var reader = new StreamReader(
            Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _sender.Send(
            new HandleStripeWebhookCommand(payload, signature),
            cancellationToken);

        return Ok();
    }
}
