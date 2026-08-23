using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record ForgotPasswordCommand(
    string Email
) : IRequest<Unit>;

public sealed class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, Unit> {
    private readonly IAuthService _authService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IAppSettings _appSettings;

    public ForgotPasswordCommandHandler(
        IAuthService authService,
        IPasswordService passwordService,
        IEmailService emailService,
        IAppSettings appSettings) {
        _authService = authService;
        _passwordService = passwordService;
        _emailService = emailService;
        _appSettings = appSettings;
    }

    public async Task<Unit> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken) {
        var emailExists = await _authService.EmailExistsAsync(
            request.Email, cancellationToken);

        if (!emailExists)
            return Unit.Value;

        var token = await _passwordService.GeneratePasswordResetTokenAsync(
            request.Email, cancellationToken);

        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(request.Email);
        var resetLink = $"{_appSettings.FrontendUrl}/auth/reset-password?token={encodedToken}&email={encodedEmail}";

        await _emailService.SendPasswordResetEmailAsync(
            request.Email, resetLink, cancellationToken);

        return Unit.Value;
    }
}
