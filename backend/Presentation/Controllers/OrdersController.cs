using Application.Features.Orders.Commands;
using Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;
using Presentation.Contracts.Orders;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase {
    private readonly ISender _sender;

    public OrdersController(ISender sender) {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new CreateOrderCommand(
                request.EventId,
                User.GetUserId(),
                request.Quantity),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] string? statusFilter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) {
        var result = await _sender.Send(
            new GetMyOrdersQuery(
                User.GetUserId(),
                statusFilter ?? string.Empty,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? statusFilter,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) {
        var result = await _sender.Send(
            new GetAllOrdersQuery(
                search ?? string.Empty,
                statusFilter ?? string.Empty,
                dateFrom,
                dateTo,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetOrderByIdQuery(
                id,
                User.GetUserId(),
                User.IsAdmin()),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new CancelOrderCommand(
                id,
                User.GetUserId(),
                User.IsAdmin()),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new RefundOrderCommand(id),
            cancellationToken);

        return NoContent();
    }
}
