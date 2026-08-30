using Application.Common;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Admin.Commands;

public sealed record ResetUserPasswordCommand(
    Guid TargetUserId
) : IRequest<Unit>;

public sealed class ResetUserPasswordCommandHandler
    : IRequestHandler<ResetUserPasswordCommand, Unit> {
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IAppSettings _appSettings;

    public ResetUserPasswordCommandHandler(
        IUserService userService,
        IPasswordService passwordService,
        IEmailService emailService,
        IAppSettings appSettings) {
        _userService = userService;
        _passwordService = passwordService;
        _emailService = emailService;
        _appSettings = appSettings;
    }

    public async Task<Unit> Handle(
        ResetUserPasswordCommand request,
        CancellationToken cancellationToken) {
        var user = await _userService.GetByIdAsync(
            request.TargetUserId, cancellationToken);

        Guard.AgainstNotFound(user, "User", request.TargetUserId);

        var token = await _passwordService.GeneratePasswordResetTokenAsync(
            user.Email, cancellationToken);

        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email);
        var resetLink =
            $"{_appSettings.FrontendUrl.TrimEnd('/')}/auth/reset-password?token={encodedToken}&email={encodedEmail}";

        await _emailService.SendPasswordResetEmailAsync(
            user.Email, resetLink, cancellationToken);

        return Unit.Value;
    }
}
