using Application.Features.Events.Commands;
using Application.Features.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;
using Presentation.Contracts.Events;
using Presentation.Extensions;

namespace Presentation.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase {
    private readonly ISender _sender;

    public EventsController(ISender sender) {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? hasAvailableTickets,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) {
        var result = await _sender.Send(
            new GetAllEventsQuery(
                search ?? string.Empty,
                categoryId,
                dateFrom,
                dateTo,
                minPrice,
                maxPrice,
                hasAvailableTickets,
                string.IsNullOrWhiteSpace(sortBy) ? "date" : sortBy,
                descending,
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
            new GetEventByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new CreateEventCommand(
                request.Title,
                request.Description,
                request.CategoryId,
                request.Location,
                request.Date,
                request.StartTime,
                request.EndTime,
                request.MaximumCapacity,
                request.TicketPrice,
                User.GetUserId(),
                request.AllowCancellation,
                request.Latitude,
                request.Longitude),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new UpdateEventCommand(
                id,
                request.Title,
                request.Description,
                request.CategoryId,
                request.Location,
                request.Date,
                request.StartTime,
                request.EndTime,
                request.MaximumCapacity,
                request.TicketPrice,
                request.AllowCancellation,
                request.Latitude,
                request.Longitude),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new DeleteEventCommand(id),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new PublishEventCommand(id),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new UnpublishEventCommand(id),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost("{id:guid}/banner")]
    public async Task<IActionResult> UploadBanner(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken) {
        if (file is null) {
            return BadRequest(new {
                code = "ValidationFailed",
                message = "An image file is required."
            });
        }

        await using var stream = file.OpenReadStream();
        var result = await _sender.Send(
            new UploadEventBannerCommand(
                id,
                stream,
                file.FileName,
                file.ContentType,
                file.Length),
            cancellationToken);

        return Ok(result);
    }
}
