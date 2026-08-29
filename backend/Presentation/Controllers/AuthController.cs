using System.Security.Claims;
using Application.Features.Auth.Commands;
using Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Contracts.Auth;

namespace Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase {
    private readonly ISender _sender;

    public AuthController(ISender sender) {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new RegisterCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.ConfirmPassword),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new LoginCommand(
                request.Email,
                request.Password,
                request.RememberMe),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new LogoutCommand(request.RefreshToken),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new ConfirmEmailCommand(request.Email, request.Token),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new ForgotPasswordCommand(request.Email),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new ResetPasswordCommand(
                request.Email,
                request.Token,
                request.NewPassword,
                request.ConfirmNewPassword),
            cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken) {
        await _sender.Send(
            new ChangePasswordCommand(
                GetUserId(),
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword),
            cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) {
        var result = await _sender.Send(
            new GetCurrentUserQuery(GetUserId()),
            cancellationToken);

        return Ok(result);
    }

    private Guid GetUserId() {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId)) {
            throw new UnauthorizedAccessException("The access token has no user id.");
        }

        return userId;
    }
}
