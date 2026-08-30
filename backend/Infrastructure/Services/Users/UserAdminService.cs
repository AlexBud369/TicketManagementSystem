using Application.Common.Models;
using Application.DTOs.Users;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using AutoMapper;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Identity;
using Infrastructure.Services.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Users;

public sealed class UserAdminService : IUserAdminService {
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuditLogWriter _auditLogWriter;
    private readonly IMapper _mapper;

    public UserAdminService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        AuditLogWriter auditLogWriter,
        IMapper mapper) {
        _dbContext = dbContext;
        _userManager = userManager;
        _auditLogWriter = auditLogWriter;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserListDto>> GetAllUsersAsync(
        string search,
        string roleFilter,
        string statusFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) {
        var query = _dbContext.AppUsers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim();
            query = query.Where(user =>
                user.Email.Contains(term) ||
                user.FirstName.Contains(term) ||
                user.LastName.Contains(term));
        }

        if (Enum.TryParse<UserStatus>(statusFilter, ignoreCase: true, out var status)) {
            query = query.Where(user => user.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(roleFilter)) {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter.Trim());
            var roleUserIds = usersInRole.Select(user => user.Id).ToList();
            query = query.Where(user => roleUserIds.Contains(user.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var profiles = await query
            .OrderByDescending(user => user.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = profiles.Select(user => user.Id).ToList();

        var orderCounts = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => userIds.Contains(order.UserId))
            .GroupBy(order => order.UserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken);

        var roleRows = await (
            from userRole in _dbContext.UserRoles
            join role in _dbContext.Roles on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId)
            select new { userRole.UserId, role.Name }
        ).ToListAsync(cancellationToken);

        var rolesByUser = roleRows
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(row => row.Name ?? string.Empty)
                    .Where(name => name.Length > 0)
                    .ToArray());

        var items = profiles.Select(profile => _mapper.Map<UserListDto>(new UserListDtoSource {
            User = profile,
            Roles = rolesByUser.GetValueOrDefault(profile.Id, []),
            OrdersCount = orderCounts.GetValueOrDefault(profile.Id)
        }));

        return PagedResult.Create(items, totalCount, page, pageSize);
    }

    public async Task DisableUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var profile = await RequireProfileAsync(userId, cancellationToken);
        profile.Status = UserStatus.Disabled;
        _auditLogWriter.Append(
            AuditAction.UserDisabled,
            "User",
            userId.ToString(),
            userId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task EnableUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var profile = await RequireProfileAsync(userId, cancellationToken);
        profile.Status = UserStatus.Active;
        _auditLogWriter.Append(
            AuditAction.UserEnabled,
            "User",
            userId.ToString(),
            userId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var profile = await RequireProfileAsync(userId, cancellationToken);

        var hasOrders = await _dbContext.Orders
            .AnyAsync(order => order.UserId == userId, cancellationToken);
        if (hasOrders) {
            throw AppException.BusinessRule(
                "User cannot be deleted because they have existing orders.");
        }

        var hasTickets = await _dbContext.Tickets
            .AnyAsync(ticket => ticket.UserId == userId, cancellationToken);
        if (hasTickets) {
            throw AppException.BusinessRule(
                "User cannot be deleted because they have existing tickets.");
        }

        var organizesEvents = await _dbContext.Events
            .AnyAsync(eventEntity => eventEntity.OrganizerId == userId, cancellationToken);
        if (organizesEvents) {
            throw AppException.BusinessRule(
                "User cannot be deleted because they organize events.");
        }

        profile.AvatarImageId = null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var uploadedImages = await _dbContext.Images
            .Where(image => image.UploadedByUserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var image in uploadedImages) {
            image.UploadedByUserId = null;
        }

        _dbContext.AppUsers.Remove(profile);
        _auditLogWriter.Append(
            AuditAction.UserDeleted,
            "User",
            userId.ToString(),
            userId: null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is not null) {
            var deleteResult = await _userManager.DeleteAsync(identityUser);
            if (!deleteResult.Succeeded) {
                throw AppException.BusinessRule(
                    string.Join(" ", deleteResult.Errors.Select(error => error.Description)));
            }
        }
    }

    public async Task ChangeUserRoleAsync(
        Guid userId,
        string newRole,
        CancellationToken cancellationToken = default) {
        if (newRole is not (AppRoles.Admin or AppRoles.User)) {
            throw AppException.BusinessRule("Role must be either 'Admin' or 'User'.");
        }

        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null) {
            throw AppException.NotFound("User", userId);
        }

        var currentRoles = await _userManager.GetRolesAsync(identityUser);
        if (currentRoles.Count > 0) {
            var removeResult = await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
            if (!removeResult.Succeeded) {
                throw AppException.BusinessRule(
                    string.Join(" ", removeResult.Errors.Select(error => error.Description)));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(identityUser, newRole);
        if (!addResult.Succeeded) {
            throw AppException.BusinessRule(
                string.Join(" ", addResult.Errors.Select(error => error.Description)));
        }

        _auditLogWriter.Append(
            AuditAction.RoleChanged,
            "User",
            userId.ToString(),
            userId,
            oldValues: string.Join(',', currentRoles),
            newValues: newRole);
        await _auditLogWriter.SaveAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(
        Guid userId,
        string firstName,
        string lastName,
        string phone,
        string address,
        CancellationToken cancellationToken = default) {
        var profile = await RequireProfileAsync(userId, cancellationToken);

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

    public async Task<IEnumerable<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null) {
            throw AppException.NotFound("User", userId);
        }

        return await _userManager.GetRolesAsync(identityUser);
    }

    private async Task<Domain.Entities.User> RequireProfileAsync(
        Guid userId,
        CancellationToken cancellationToken) {
        var profile = await _dbContext.AppUsers
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (profile is null) {
            throw AppException.NotFound("User", userId);
        }

        return profile;
    }
}
