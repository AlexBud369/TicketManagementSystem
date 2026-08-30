using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Identity;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Seeding;

public static class IdentitySeeder {
    public static async Task SeedAsync(IServiceProvider services) {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentitySeeder");
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = provider.GetRequiredService<AppDbContext>();
        var seedOptions = provider.GetRequiredService<IOptions<SeedOptions>>().Value;

        await EnsureRoleAsync(roleManager, AppRoles.Admin);
        await EnsureRoleAsync(roleManager, AppRoles.User);

        if (string.IsNullOrWhiteSpace(seedOptions.AdminEmail) ||
            string.IsNullOrWhiteSpace(seedOptions.AdminPassword)) {
            logger.LogInformation(
                "Admin seed skipped. Set Seed:AdminEmail and Seed:AdminPassword to create the admin user.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(seedOptions.AdminEmail);
        if (existingAdmin is not null) {
            logger.LogInformation(
                "Admin user {Email} already exists.", seedOptions.AdminEmail);
            return;
        }

        var adminId = Guid.NewGuid();
        var admin = new ApplicationUser {
            Id = adminId,
            UserName = seedOptions.AdminEmail,
            Email = seedOptions.AdminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(admin, seedOptions.AdminPassword);
        if (!createResult.Succeeded) {
            var errors = string.Join(
                "; ", createResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException(
                $"Failed to seed admin user: {errors}");
        }

        await userManager.AddToRoleAsync(admin, AppRoles.Admin);

        var profileExists = await dbContext.AppUsers
            .AnyAsync(user => user.Id == adminId);

        if (!profileExists) {
            dbContext.AppUsers.Add(new User {
                Id = adminId,
                FirstName = seedOptions.AdminFirstName,
                LastName = seedOptions.AdminLastName,
                Email = seedOptions.AdminEmail,
                Status = UserStatus.Active
            });

            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation("Seeded admin user {Email}.", seedOptions.AdminEmail);
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName) {
        if (await roleManager.RoleExistsAsync(roleName)) {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        if (!result.Succeeded) {
            var errors = string.Join(
                "; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(
                $"Failed to seed role '{roleName}': {errors}");
        }
    }
}
