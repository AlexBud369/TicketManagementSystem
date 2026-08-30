using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterResponse>;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, RegisterResponse> {
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IAppSettings _appSettings;

    public RegisterCommandHandler(
        IAuthService authService,
        IUserService userService,
        IPasswordService passwordService,
        IEmailService emailService,
        IAppSettings appSettings) {
        _authService = authService;
        _userService = userService;
        _passwordService = passwordService;
        _emailService = emailService;
        _appSettings = appSettings;
    }

    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken) {
        var emailExists = await _authService.EmailExistsAsync(
            request.Email, cancellationToken);

        Guard.AgainstDuplicate(emailExists, "Email is already registered.");

        var userId = await _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            cancellationToken);

        var user = await _userService.GetByIdAsync(userId, cancellationToken);

        var token = await _passwordService.GenerateEmailConfirmationTokenAsync(
            userId, cancellationToken);

        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email);
        var confirmationLink =
            $"{_appSettings.FrontendUrl.TrimEnd('/')}/auth/confirm-email?token={encodedToken}&email={encodedEmail}";

        await _emailService.SendRegistrationEmailAsync(
            user.Email,
            user.FullName,
            confirmationLink,
            cancellationToken);

        return new RegisterResponse {
            UserId = user.Id,
            Email = user.Email,
            RequiresEmailConfirmation = true
        };
    }
}
