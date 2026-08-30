using Application.Features.Admin.Commands;
using Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;
using Presentation.Contracts.Admin;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Authorize(Roles = AppRoleNames.Admin)]
[ApiController]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase {
    private readonly ISender _sender;

    public AdminUsersController(ISender sender) {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? roleFilter,
        [FromQuery] string? statusFilter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) {
        var result = await _sender.Send(
            new GetAllUsersQuery(
                search ?? string.Empty,
                roleFilter ?? string.Empty,
                statusFilter ?? string.Empty,
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
            new GetUserByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserByAdminRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new UpdateUserByAdminCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Phone,
                request.Address),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> Disable(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new DisableUserCommand(User.GetUserId(), id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/enable")]
    public async Task<IActionResult> Enable(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new EnableUserCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new DeleteUserCommand(User.GetUserId(), id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new ChangeUserRoleCommand(
                User.GetUserId(),
                id,
                request.NewRole),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new ResetUserPasswordCommand(id),
            cancellationToken);

        return NoContent();
    }
}
