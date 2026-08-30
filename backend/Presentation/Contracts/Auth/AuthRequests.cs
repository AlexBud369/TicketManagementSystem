namespace Presentation.Contracts.Auth;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword);

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record LogoutRequest(
    string RefreshToken);

public sealed record ConfirmEmailRequest(
    string Email,
    string Token);

public sealed record ForgotPasswordRequest(
    string Email);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
