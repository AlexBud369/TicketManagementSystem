namespace Application.DTOs.Auth;

public sealed class RegisterResponse {
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool RequiresEmailConfirmation { get; init; } = true;
}
