using System.Text;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence.Identity;
using Infrastructure.Services.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Infrastructure.Services.Auth;

public sealed class PasswordService : IPasswordService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuditLogWriter _auditLogWriter;

    public PasswordService(
        UserManager<ApplicationUser> userManager,
        AuditLogWriter auditLogWriter) {
        _userManager = userManager;
        _auditLogWriter = auditLogWriter;
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default) {
        var identityUser = await RequireUserByIdAsync(userId);

        var changed = await _userManager.ChangePasswordAsync(
            identityUser, currentPassword, newPassword);

        if (!changed.Succeeded) {
            if (changed.Errors.Any(error => error.Code == "PasswordMismatch")) {
                throw AppException.Unauthorized("Current password is incorrect.");
            }

            throw AppException.BusinessRule(
                string.Join(" ", changed.Errors.Select(error => error.Description)));
        }

        _auditLogWriter.Append(
            AuditAction.PasswordChanged,
            "User",
            userId.ToString(),
            userId);
        await _auditLogWriter.SaveAsync(cancellationToken);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default) {
        var identityUser = await RequireUserByEmailAsync(email);
        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
        return EncodeToken(token);
    }

    public async Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default) {
        var identityUser = await RequireUserByEmailAsync(email);

        var reset = await _userManager.ResetPasswordAsync(
            identityUser, DecodeToken(token), newPassword);

        if (!reset.Succeeded) {
            throw AppException.BusinessRule(
                "Invalid or expired password reset token.");
        }

        _auditLogWriter.Append(
            AuditAction.PasswordReset,
            "User",
            identityUser.Id.ToString(),
            identityUser.Id);
        await _auditLogWriter.SaveAsync(cancellationToken);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var identityUser = await RequireUserByIdAsync(userId);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
        return EncodeToken(token);
    }

    public async Task ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default) {
        var identityUser = await RequireUserByIdAsync(userId);

        var confirmed = await _userManager.ConfirmEmailAsync(
            identityUser, DecodeToken(token));

        if (!confirmed.Succeeded) {
            throw AppException.BusinessRule(
                "Invalid or expired email confirmation token.");
        }
    }

    private async Task<ApplicationUser> RequireUserByIdAsync(Guid userId) {
        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null) {
            throw AppException.NotFound("User", userId);
        }

        return identityUser;
    }

    private async Task<ApplicationUser> RequireUserByEmailAsync(string email) {
        var identityUser = await _userManager.FindByEmailAsync(email.Trim());
        if (identityUser is null) {
            throw AppException.NotFound("User with this email was not found.");
        }

        return identityUser;
    }

    private static string EncodeToken(string token) {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    private static string DecodeToken(string token) {
        var normalized = Uri.UnescapeDataString(token.Trim()).Replace(' ', '+');

        try {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(normalized));
        }
        catch (FormatException) {
            return normalized;
        }
    }
}
