namespace Application.Interfaces;

public interface IPasswordService {
    Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<string> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<string> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);
}
