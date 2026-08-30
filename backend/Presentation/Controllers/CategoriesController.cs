using Application.Features.Categories.Commands;
using Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Authorization;
using Presentation.Contracts.Categories;

namespace Presentation.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase {
    private readonly ISender _sender;

    public CategoriesController(ISender sender) {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetAllCategoriesQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetCategoryByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new CreateCategoryCommand(
                request.Name,
                request.Description,
                request.Icon),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new UpdateCategoryCommand(
                id,
                request.Name,
                request.Description,
                request.Icon),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = AppRoleNames.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new DeleteCategoryCommand(id),
            cancellationToken);

        return NoContent();
    }
}
