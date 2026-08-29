using Application.DTOs.Users;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Identity;
using Infrastructure.Services.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Auth;

public sealed class AuthService : IAuthService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly AuditLogWriter _auditLogWriter;
    private readonly UserMapper _userMapper;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        AuditLogWriter auditLogWriter,
        UserMapper userMapper) {
        _userManager = userManager;
        _dbContext = dbContext;
        _auditLogWriter = auditLogWriter;
        _userMapper = userMapper;
    }

    public async Task<Guid> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default) {
        var userId = Guid.NewGuid();
        var normalizedEmail = email.Trim();

        var identityUser = new ApplicationUser {
            Id = userId,
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(identityUser, password);
        if (!createResult.Succeeded) {
            ThrowIdentityErrors(createResult);
        }

        var roleResult = await _userManager.AddToRoleAsync(identityUser, AppRoles.User);
        if (!roleResult.Succeeded) {
            await _userManager.DeleteAsync(identityUser);
            ThrowIdentityErrors(roleResult);
        }

        _dbContext.AppUsers.Add(new User {
            Id = userId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = normalizedEmail,
            Status = UserStatus.Active
        });

        _auditLogWriter.Append(
            AuditAction.UserCreated,
            nameof(User),
            userId.ToString(),
            userId);

        try {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch {
            await _userManager.DeleteAsync(identityUser);
            throw;
        }

        return userId;
    }

    public async Task<UserDto> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default) {
        var identityUser = await _userManager.FindByEmailAsync(email.Trim());
        if (identityUser is null) {
            throw AppException.Unauthorized("Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(identityUser)) {
            throw AppException.BusinessRule(
                "This account is locked. Try again later.");
        }

        if (!await _userManager.CheckPasswordAsync(identityUser, password)) {
            await _userManager.AccessFailedAsync(identityUser);
            throw AppException.Unauthorized("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(identityUser);

        var profile = await _dbContext.AppUsers
            .AsNoTracking()
            .Include(user => user.AvatarImage)
            .FirstOrDefaultAsync(user => user.Id == identityUser.Id, cancellationToken);

        if (profile is null) {
            throw AppException.NotFound("User", identityUser.Id);
        }

        _auditLogWriter.Append(
            AuditAction.Login,
            nameof(User),
            identityUser.Id.ToString(),
            identityUser.Id);
        await _auditLogWriter.SaveAsync(cancellationToken);

        return await _userMapper.ToDtoAsync(profile, identityUser, _userManager);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default) {
        var identityUser = await _userManager.FindByEmailAsync(email.Trim());
        return identityUser is not null;
    }

    private static void ThrowIdentityErrors(IdentityResult result) {
        if (result.Errors.Any(error =>
            error.Code is "DuplicateEmail" or "DuplicateUserName")) {
            throw AppException.Conflict("Email is already registered.");
        }

        var messages = result.Errors
            .Select(error => error.Description)
            .ToArray();

        throw AppException.BusinessRule(string.Join(" ", messages));
    }
}
