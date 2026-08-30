using Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts.Users;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase {
    private readonly ISender _sender;

    public UsersController(ISender sender) {
        _sender = sender;
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new UpdateProfileCommand(
                User.GetUserId(),
                request.FirstName,
                request.LastName,
                request.Phone,
                request.Address),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar(
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
            new UploadAvatarCommand(
                User.GetUserId(),
                stream,
                file.FileName,
                file.ContentType,
                file.Length),
            cancellationToken);

        return Ok(result);
    }
}
