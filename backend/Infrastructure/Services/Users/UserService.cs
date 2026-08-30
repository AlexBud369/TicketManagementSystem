using Application.DTOs.Users;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Identity;
using Infrastructure.Services.Audit;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Users;

public sealed class UserService : IUserService {
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuditLogWriter _auditLogWriter;
    private readonly UserMapper _userMapper;

    public UserService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        AuditLogWriter auditLogWriter,
        UserMapper userMapper) {
        _dbContext = dbContext;
        _userManager = userManager;
        _auditLogWriter = auditLogWriter;
        _userMapper = userMapper;
    }

    public async Task<UserDto> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var profile = await _dbContext.AppUsers
            .AsNoTracking()
            .Include(user => user.AvatarImage)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (profile is null) {
            throw AppException.NotFound("User", userId);
        }

        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null) {
            throw AppException.NotFound("User", userId);
        }

        return await _userMapper.ToDtoAsync(profile, identityUser, _userManager);
    }

    public async Task<UserDto> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default) {
        var identityUser = await _userManager.FindByEmailAsync(email.Trim());
        if (identityUser is null) {
            throw AppException.NotFound("User with this email was not found.");
        }

        return await GetByIdAsync(identityUser.Id, cancellationToken);
    }

    public async Task UpdateProfileAsync(
        Guid userId,
        string firstName,
        string lastName,
        string phone,
        string address,
        CancellationToken cancellationToken = default) {
        var profile = await _dbContext.AppUsers
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (profile is null) {
            throw AppException.NotFound("User", userId);
        }

        profile.FirstName = firstName.Trim();
        profile.LastName = lastName.Trim();
        profile.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        profile.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        _auditLogWriter.Append(
            AuditAction.UserUpdated,
            "User",
            userId.ToString(),
            userId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAvatarAsync(
        Guid userId,
        Guid imageId,
        CancellationToken cancellationToken = default) {
        var profile = await _dbContext.AppUsers
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (profile is null) {
            throw AppException.NotFound("User", userId);
        }

        var imageExists = await _dbContext.Images
            .AnyAsync(image => image.Id == imageId, cancellationToken);

        if (!imageExists) {
            throw AppException.NotFound("Image", imageId);
        }

        profile.AvatarImageId = imageId;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
